using System;
using System.IO;
using NavisAutoClash.Core.Application.Contracts;

namespace NavisAutoClash.Infrastructure.Logging
{
    /// <summary>
    /// Simple file-based logger writing to %APPDATA%\NavisAutoClash\logs\navisautoclash.log.
    /// Thread-safe via lock. No external dependencies required for this lightweight implementation.
    /// </summary>
    public sealed class FileLogger : IAppLogger
    {
        private readonly string _logFilePath;
        private readonly object _lock = new object();

        public FileLogger()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var logDir = Path.Combine(appData, "NavisAutoClash", "logs");

            // SAFETY: Only ever writes inside %APPDATA%\NavisAutoClash — never outside.
            Directory.CreateDirectory(logDir);
            _logFilePath = Path.Combine(logDir, "navisautoclash.log");
        }

        /// <inheritdoc/>
        public void Info(string message) => Write("INFO ", message, null);

        /// <inheritdoc/>
        public void Warn(string message) => Write("WARN ", message, null);

        /// <inheritdoc/>
        public void Error(string message, Exception? ex = null) => Write("ERROR", message, ex);

        // ── private helpers ────────────────────────────────────────────────────

        private void Write(string level, string message, Exception? ex)
        {
            try
            {
                var entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
                if (ex != null)
                    entry += $"{Environment.NewLine}  Exception: {ex.GetType().Name}: {ex.Message}";

                lock (_lock)
                {
                    File.AppendAllText(_logFilePath, entry + Environment.NewLine);
                }
            }
            catch
            {
                // Never let logging crash the host application
            }
        }
    }
}
