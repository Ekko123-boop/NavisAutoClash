using System;

namespace NavisAutoClash.Core.Application.Contracts
{
    /// <summary>
    /// Marshals actions onto the Navisworks UI thread.
    /// Navisworks API calls are not thread-safe and must run on the thread
    /// that owns the document (the WPF dispatcher thread).
    /// </summary>
    public interface INavisDispatcher
    {
        /// <summary>Synchronously invokes <paramref name="action"/> on the UI thread.</summary>
        void Invoke(Action action);

        /// <summary>Synchronously invokes <paramref name="func"/> on the UI thread and returns its result.</summary>
        T Invoke<T>(Func<T> func);
    }
}
