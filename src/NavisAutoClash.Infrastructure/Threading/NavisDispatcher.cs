using System;
using System.Threading.Tasks;
using System.Windows;
using NavisAutoClash.Core.Application.Contracts;

namespace NavisAutoClash.Infrastructure.Threading
{
    public class NavisDispatcher : INavisDispatcher
    {
        public void Invoke(Action action)
        {
            if (Application.Current?.Dispatcher != null)
            {
                Application.Current.Dispatcher.Invoke(action);
            }
            else
            {
                action();
            }
        }

        public T Invoke<T>(Func<T> func)
        {
            if (Application.Current?.Dispatcher != null)
            {
                return Application.Current.Dispatcher.Invoke(func);
            }
            return func();
        }

        public async Task InvokeAsync(Action action)
        {
            if (Application.Current?.Dispatcher != null)
            {
                await Application.Current.Dispatcher.InvokeAsync(action);
            }
            else
            {
                action();
            }
        }

        public async Task<T> InvokeAsync<T>(Func<T> func)
        {
            if (Application.Current?.Dispatcher != null)
            {
                return await Application.Current.Dispatcher.InvokeAsync(func);
            }
            return func();
        }
    }
}
