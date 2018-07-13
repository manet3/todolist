using System;
using System.ComponentModel;

namespace ToDoList.Client
{
    public class TaskEventArgs : EventArgs
    {
        public bool IsSelectedChanged;
        public bool IsCheckedChanged;
        public TaskEventArgs(bool selecetedChanged, bool checkedChanged)
        {
            IsSelectedChanged = selecetedChanged;
            IsCheckedChanged = checkedChanged;
        }
    }

    class TaskVM : INotifyPropertyChanged
    {
        public TaskModel Model;

        public static event EventHandler<TaskEventArgs> TaskChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsDone
        {
            get => Model.State;
            set
            {
                Model.State = value;
                TaskChanged?.Invoke(this, new TaskEventArgs(false, true));
                OnPropertyChanged(nameof(IsDone));
            }
        }

        public bool IsSelected
        {
            get => _isSelected; set
            {
                _isSelected = value;
                TaskChanged?.Invoke(this, new TaskEventArgs(true, false));
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        public string Content { get; set; }

        #region generic fields
        private bool _isSelected;
        #endregion

        private TaskVM()
        {
        }

        public TaskVM(string content, bool state, ulong index) : this()
        {
            Content = content;
            Model = new TaskModel(content, state, index);
        }

        public TaskVM(string content, bool state) : this()
        {
            Content = content;
            Model = new TaskModel(content, state, 0);
        }

        public TaskVM(TaskModel model) : this()
        {
            Content = model.Name;
            Model = model;
        }

        public void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

    }
}
