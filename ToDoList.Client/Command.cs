using System;
using System.Windows.Input;

namespace ToDoList.Client
{
    class Command : ICommand
    {
        Action<object> _action;
        Func<bool> _callable;

        public event EventHandler CanExecuteChanged;

        public Command(Action<object> handler)
            => _action = handler;

        public Command(Action<object> handler, Func<bool> callable) : this(handler)
            => _callable = callable;

        public void RaiseExecuteChanged()
            => CanExecuteChanged?.Invoke(this, new EventArgs());

        public bool CanExecute(object parameter)
        {
            if (_callable != null)
                return _callable();
            return true;
        }

        public void Execute(object parameter)
        {
            if (CanExecute(parameter))
                _action(parameter);
        }
    }
}
