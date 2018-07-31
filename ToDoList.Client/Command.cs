using System;
using System.Windows.Input;

namespace ToDoList.Client
{
    public class Command : ICommand
    {
        Action<object> _execute;
        Func<bool> _canExecute;

        public event EventHandler CanExecuteChanged;

        public Command(Action<object> execute, Func<bool> canExecute = default)
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));

            (_execute, _canExecute) = (execute, canExecute);
        }

        public void RaiseExecuteChanged()
            => CanExecuteChanged?.Invoke(this, new EventArgs());

        public bool CanExecute(object parameter)
            => _canExecute?.Invoke() ?? true;

        public void Execute(object parameter)
            => _execute(parameter);
    }
}
