using System;
using System.Windows.Input;

namespace NavisAutoClash.UI.Infrastructure
{
    /// <summary>
    /// Simple <see cref="ICommand"/> implementation for synchronous commands.
    /// Avoids requiring CommunityToolkit generator for one-off cases.
    /// </summary>
    public sealed class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
            : this(_ => execute(), canExecute is null ? null : (Func<object?, bool>)(_ => canExecute()))
        { }

        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;
        public void Execute(object? parameter) => _execute(parameter);

        /// <summary>Forces a CanExecute re-evaluation on the UI thread.</summary>
        public void RaiseCanExecuteChanged() =>
            System.Windows.Application.Current?.Dispatcher.Invoke(CommandManager.InvalidateRequerySuggested);
    }
}
