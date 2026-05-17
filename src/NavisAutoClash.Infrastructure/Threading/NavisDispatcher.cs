using System;
using System.Windows;
using NavisAutoClash.Core.Application.Contracts;

namespace NavisAutoClash.Infrastructure.Threading
{
    /// <summary>
    /// Marshals calls onto the WPF dispatcher thread, which is also the Navisworks UI thread.
    /// Falls back to direct invocation if no WPF Application is running (e.g. unit tests).
    /// </summary>
    public sealed class NavisDispatcher : INavisDispatcher
    {
        /// <inheritdoc/>
        public void Invoke(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher != null && !dispatcher.CheckAccess())
                dispatcher.Invoke(action);
            else
                action();
        }

        /// <inheritdoc/>
        public T Invoke<T>(Func<T> func)
        {
            if (func == null) throw new ArgumentNullException(nameof(func));

            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher != null && !dispatcher.CheckAccess())
                return dispatcher.Invoke(func);
            else
                return func();
        }
    }
}
