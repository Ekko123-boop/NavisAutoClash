using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using NavisAutoClash.Core.Application.Contracts;
using NavisAutoClash.Core.Application.UseCases;
using NavisAutoClash.Infrastructure.Logging;
using NavisAutoClash.Infrastructure.Repositories;
using NavisAutoClash.Infrastructure.Services;
using NavisAutoClash.Infrastructure.Threading;
using NavisAutoClash.UI.ViewModels;
using NavisAutoClash.UI.Views;

namespace NavisAutoClash.Plugin
{
    /// <summary>
    /// Wires the DI container, resolves the root ViewModel, and shows the main window.
    /// Isolated from <see cref="AutoClashCommand"/> so the JIT only touches these
    /// external types after the assembly resolver is active.
    /// </summary>
    internal sealed class Bootstrapper
    {
        public int Run()
        {
            var services = BuildServiceContainer();

            using var scope = services.CreateScope();
            var viewModel = scope.ServiceProvider.GetRequiredService<MainViewModel>();
            var window = new MainWindow(viewModel);

            // Attach to Navisworks main window so the dialog stays on top.
            try
            {
                var helper = new System.Windows.Interop.WindowInteropHelper(window);
                helper.Owner = Autodesk.Navisworks.Api.Application.Gui.MainWindow.Handle;
            }
            catch (Exception ex)
            {
                // Non-fatal — window will still open; just may not be modal to Navisworks
                scope.ServiceProvider.GetService<IAppLogger>()?.Warn(
                    $"Could not set window owner: {ex.Message}");
            }

            window.ShowDialog();
            return 0;
        }

        private static ServiceProvider BuildServiceContainer()
        {
            var services = new ServiceCollection();

            // ── Infrastructure (singletons — shared for the lifetime of the window) ──
            services.AddSingleton<IAppLogger, FileLogger>();
            services.AddSingleton<INavisDispatcher, NavisDispatcher>();
            services.AddSingleton<ISelectionSetRepository, NavisSelectionSetRepository>();
            services.AddSingleton<IModelRepository, NavisModelRepository>();
            services.AddSingleton<IClashService, NavisClashService>();

            // ── Use Cases (transient — stateless) ─────────────────────────────────
            services.AddTransient<GenerateClashTestsUseCase>();

            // ── UI ViewModels (transient — fresh per window) ──────────────────────
            services.AddTransient<MainViewModel>();

            return services.BuildServiceProvider();
        }
    }
}
