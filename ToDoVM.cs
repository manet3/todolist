using System;
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
    class ToDoVM: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region generic_fields
        private ObservableCollection<TaskVM> _toDo;
        private LoadingState _loaderState;
        string _toDoItem;
        string _errorMesage;
        #endregion

        #region ICommands
        public ICommand AddCommand { get; set; } 
        public ICommand RemoveCommand { get; set; }
        public ICommand RestartCommand { get; set; }
        #endregion

        public string ToDoItem
        {
            get => _toDoItem;
            set
            {
                _toDoItem = value;
                OnPropertyChanged(nameof(ToDoItem));
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
            get => _toDo; private set
            {
                _toDo = value;
                OnPropertyChanged(nameof(ToDo));
            }
        }

        public string ErrorMesage
        {
            get => _errorMesage;
            set
            {
                _errorMesage = value;
                OnPropertyChanged(nameof(ErrorMesage));
            }
        }

        private List<TaskVM> Selected = new List<TaskVM>();

        private ToDoModel model;

        public ToDoVM()
        {
            AddCommand = new Command(ToDoAdd, () => ToDoItem != null && !ToDoItem.Equals(""));
            RemoveCommand = new Command(ToDoRemove);
            RestartCommand = new Command(OnRestart);
            ToDo = new ObservableCollection<TaskVM>();
            model = new ToDoModel();
            TaskVM.TaskChanged += OnTaskChanged;
            Synchronisator.SynchChanged += SyncHandler;
            ToDoItemsGetAsync();
        }

        private void OnRestart(object obj)
        {
            ToDoItemsGetAsync();
        }

        private void SyncHandler(SyncState state)
        {
            //instead of direct convertation
            switch(state)
            {
                case SyncState.Failed:
                    ErrorMesage = "Failed to connect the server." +
                        " Check your internet connection and try again.";
                    LoaderState = LoadingState.Failed;
                    break;
                case SyncState.Started:
                    ErrorMesage = "";
                    LoaderState = LoadingState.Started;break;
                default:
                    ErrorMesage = "";
                    LoaderState = LoadingState.None;break;
            }
        }

        private async Task ToDoItemsGetAsync()
        {
            ToDo = new ObservableCollection<TaskVM>(
                (await model.GetTasks())
                .Select(x=> new TaskVM(x)));
        }

        private void OnTaskChanged(object sender, TaskEventArgs e)
        {
            var task = (TaskVM)sender;
            //update changed properties
            if (e.IsCheckedChanged)
                model.AddItemAsync(task.Model);

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
            var newItem = new TaskVM(ToDoItem, false, itemIndex);
            model.AddItemAsync(newItem.Model);
            ToDo.Add(newItem);
        }

        private void ToDoRemove(object obj)
        {
            foreach (var item in Selected)
            {
                ToDo.Remove(item);
                model.DeleteItemAsync(item.Model);
            }
            Selected.Clear();
        }

        public void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
