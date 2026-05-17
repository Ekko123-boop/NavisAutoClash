using System;
using System.Reflection;
using System.Windows;
using Autodesk.Navisworks.Api.Plugins;

namespace NavisAutoClash.Plugin
{
    /// <summary>
    /// Navisworks 2024 add-in entry point.
    /// 
    /// STABILITY PATTERN: The AssemblyResolve hook is registered BEFORE any external types
    /// are referenced, ensuring all dependencies are found next to this DLL.
    /// All business logic is deferred into <see cref="Bootstrapper"/> so that the JIT
    /// compiler does not attempt to resolve those types until the resolver is live.
    /// </summary>
    [Plugin("NavisAutoClash", "BIMDev",
        DisplayName = "Auto Clash",
        ToolTip = "Automated Clash Test Generation — NavisAutoClash")]
    [AddInPlugin(AddInLocation.AddIn)]
    public sealed class AutoClashCommand : AddInPlugin
    {
        public override int Execute(params string[] parameters)
        {
            // Step 1: Hook up assembly resolution IMMEDIATELY so all our DLLs can be found.
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;

            try
            {
                // Step 2: Run bootstrapper in a separate method body.
                // This prevents the JIT from resolving external types before the resolver is live.
                return RunBootstrapper();
            }
            catch (Exception ex)
            {
                // Last-resort handler: show a message and log without crashing Navisworks.
                TryLogStartupError(ex);
                MessageBox.Show(
                    $"NavisAutoClash failed to start:\n\n{ex.Message}\n\nSee %APPDATA%\\NavisAutoClash\\logs for details.",
                    "NavisAutoClash — Startup Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return 0;
            }
        }

        // ── separated to prevent premature JIT resolution ──────────────────────
        private static int RunBootstrapper()
        {
            var bootstrapper = new Bootstrapper();
            return bootstrapper.Run();
        }

        // ── assembly resolution fallback ───────────────────────────────────────
        private static Assembly? OnAssemblyResolve(object? sender, ResolveEventArgs args)
        {
            try
            {
                var pluginFolder = System.IO.Path.GetDirectoryName(
                    typeof(AutoClashCommand).Assembly.Location);
                if (pluginFolder == null) return null;

                var assemblyName = new AssemblyName(args.Name).Name;
                if (string.IsNullOrEmpty(assemblyName)) return null;

                var candidatePath = System.IO.Path.Combine(pluginFolder, assemblyName + ".dll");
                if (System.IO.File.Exists(candidatePath))
                    return Assembly.LoadFrom(candidatePath);
            }
            catch
            {
                // Swallow — returning null allows the CLR to continue probing
            }
            return null;
        }

        private static void TryLogStartupError(Exception ex)
        {
            try
            {
                var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var logDir = System.IO.Path.Combine(appData, "NavisAutoClash", "logs");
                System.IO.Directory.CreateDirectory(logDir);
                var logPath = System.IO.Path.Combine(logDir, "navisautoclash.log");
                System.IO.File.AppendAllText(logPath,
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [FATAL] Startup error: {ex}{Environment.NewLine}");
            }
            catch { /* never let logging crash the host */ }
        }
    }
}
