using System;
using System.Windows.Input;
using ToDoList.Shared;

namespace ToDoList.Client.ViewModels
{
    class ItemViewModel
    {
        private ToDoItem _model;

        public string Name
        {
            get => _model.Name;
            set
            {
                _model.Name = value;
                UpdateTimestamp();
            }
        }

        public bool IsChecked
        {
            get => _model.IsChecked;
            set
            {
                _model.IsChecked = value;
                UpdateTimestamp();
            }
        }

        public Action GetOnChangedAction(ICommand onChanged)
            => () =>
            {
                if (onChanged.CanExecute(this))
                    onChanged.Execute(_model);
            };

        public ItemViewModel(ToDoItem model)
            => _model = model;

        public void UpdateTimestamp()
            => _model.Timestamp = DateTime.UtcNow;
    }
}
