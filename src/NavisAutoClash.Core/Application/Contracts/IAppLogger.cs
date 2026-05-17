using System;

namespace NavisAutoClash.Core.Application.Contracts
{
    /// <summary>
    /// Simple logging abstraction. Implemented in Infrastructure (Serilog/file).
    /// </summary>
    public interface IAppLogger
    {
        void Info(string message);
        void Warn(string message);
        void Error(string message, Exception? ex = null);
    }
}
