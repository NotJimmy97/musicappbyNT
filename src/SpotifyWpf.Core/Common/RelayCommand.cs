using System;
using System.Windows.Input;

namespace SpotifyWpf.Core.Common
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public RelayCommand(Action execute, Func<bool> canExecute = null)
            : this(
                execute != null ? (Action<object>)(_ => execute()) : throw new ArgumentNullException(nameof(execute)),
                canExecute != null ? (Predicate<object>)(_ => canExecute()) : null)
        {
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            // Forces the WPF CommandManager to re-evaluate command availability across active visual elements
            CommandManager.InvalidateRequerySuggested();
        }

        public event EventHandler CanExecuteChanged
        {
            // Hook directly into WPF CommandManager requery cycle to automatically sync button states with ViewModel condition changes
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}

