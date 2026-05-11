using System;
using System.Threading.Tasks;

namespace NavisAutoClash.Core.Application.Contracts
{
    public interface INavisDispatcher
    {
        void Invoke(Action action);
        T Invoke<T>(Func<T> func);
        Task InvokeAsync(Action action);
        Task<T> InvokeAsync<T>(Func<T> func);
    }
}
