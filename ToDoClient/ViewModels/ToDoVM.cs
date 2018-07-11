using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ToDoList.Client.Controls;

namespace ToDoList.Client
{
    class ToDoVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region generic_fields
        private ObservableCollection<TaskVM> _toDo;
        private LoadingState _loaderState;
        string _toDoItem;
        ToDoNotification _notification;
        #endregion

        #region ICommands
        public ICommand AddCommand { get; set; }
        public ICommand RemoveCommand { get; set; }
        public ICommand RestartCommand { get; set; }
        public ICommand ClosingCommand { get; set; }
        #endregion

        #region updated fields
        public string ToDoItemText
        {
            get => _toDoItem;
            set
            {
                _toDoItem = value;
                OnPropertyChanged(nameof(ToDoItemText));
                ((Command)AddCommand).RaiseExecuteChanged();
            }
        }

        public LoadingState LoaderState
        {
            get => _loaderState;
            set
            {
                _loaderState = value;
                OnPropertyChanged(nameof(LoaderState));
            }
        }

        public Visibility ButtonRemoveVis
        {
            get => Selected.Count == 0
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        public ObservableCollection<TaskVM> ToDo
        {
            get => _toDo;
            private set
            {
                _toDo = value;
                OnPropertyChanged(nameof(ToDo));
            }
        }

        public ToDoNotification Notification
        {
            get => _notification;
            set
            {
                _notification = value;
                OnPropertyChanged(nameof(Notification));
            }
        }
        #endregion

        private List<TaskVM> Selected = new List<TaskVM>();

        private ToDoModel _model;

        public class ToDoNotification
        {
            public string Message { get; set; }

            public ToDoNotification(string message)
            {
                Message = message;
            }

            public static ToDoNotification None = new ToDoNotification("");

            public static ToDoNotification ServerError 
                = new ToDoNotification("Failed to connect the server." +
                        " Check your internet connection and try again.");

            public static ToDoNotification ItemExists = new ToDoNotification("Doing the same " +
                "task twice is unproductive.");

        }

        public ToDoVM()
        {
            #region Commands
            AddCommand = new Command(ToDoAdd, () => ToDoItemText != null && !string.IsNullOrWhiteSpace(ToDoItemText));
            RemoveCommand = new Command(ToDoRemove);
            RestartCommand = new Command(OnRestart);
            ClosingCommand = new Command(OnFinishing);
            #endregion
            ToDo = new ObservableCollection<TaskVM>();
            _model = new ToDoModel();
            TaskVM.TaskChanged += OnTaskChanged;
            Synchronisator.SynchChanged += SyncHandler;
            GetList();
        }

        private void OnFinishing(object obj)
        {
            _model.CheckSaveProgress();
        }

        private void GetList()
        {
            _model.GotItems += TranslateItems;
            _model.GetItems();
        }

        private void OnRestart(object obj) => _model.Retry();

        private void SyncHandler(SyncState state)
        {
            //instead of direct convertation
            switch (state)
            {
                case SyncState.Failed:
                    Notification = ToDoNotification.ServerError;
                    LoaderState = LoadingState.Failed;
                    break;
                case SyncState.Started:
                    Notification = ToDoNotification.None;
                    LoaderState = LoadingState.Started; break;
                default:
                    Notification = ToDoNotification.None;
                    LoaderState = LoadingState.None; break;
            }
        }

        private void TranslateItems()
        {
            ToDo = new ObservableCollection<TaskVM>(
                _model.ItamsData.Select(x => new TaskVM(x)));
        }

        private void OnTaskChanged(object sender, TaskEventArgs e)
        {
            var task = (TaskVM)sender;
            //update changed properties
            if (e.IsCheckedChanged)
                _model.TryAddItem(task.Model);

            if (!e.IsSelectedChanged) return;
            if (task.IsSelected)
                Selected.Add(task);
            else Selected.Remove(task);

            OnPropertyChanged(nameof(ButtonRemoveVis));
        }

        private void ToDoAdd(object obj)
        {
            var itemIndex = ToDo.Count != 0
                ? ToDo.Max(x => x.Model.Index) + 1
                : 0;
            var newItem = new TaskVM(ToDoItemText, false, itemIndex);
            if (_model.TryAddItem(newItem.Model))
            {
                ToDo.Add(newItem);
                ToDoItemText = "";
            }
            else ShowTemporalMessageAsync(ToDoNotification.ItemExists);
                
        }

        private void ToDoRemove(object obj)
        {
            foreach (var item in Selected)
            {
                ToDo.Remove(item);
                _model.TryDeleteItem(item.Model);
            }
            Selected.Clear();
        }

        private async Task ShowTemporalMessageAsync(ToDoNotification message)
        {
            Notification = message;
            await Task.Delay(new TimeSpan(0, 0, 5));
            Notification = ToDoNotification.None;
        }

        public void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
