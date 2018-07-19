using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ToDoList.Client.ViewModels
{
    class ToDoViewModel : ViewModelBase
    {
        #region ICommands
        public Command RemoveCommand { get; set; }
        #endregion

        #region auto_fields
        string _toDoItem;
        string _notification;
        #endregion

        #region notified properties
        public string ToDoItemText
        {
            get => _toDoItem;
            set
            {
                SetValue(ref _toDoItem, value);
                AddCommand.RaiseExecuteChanged();
            }
        }
        
        public ObservableCollection<TaskModel> ToDoItems { get; }

        public string NotificationMessage
        {
            get => _notification;
            set => SetValue(ref _notification, value);
        }

        #endregion

        #region error messages
        private const string MESSAGE_CLEAR = "";
        private const string MESSAGE_DUPLICATION = "Doing the same task twice is unproductive.";
        #endregion

        private readonly HashSet<TaskModel> _itemsData = new HashSet<TaskModel>();

        public ToDoViewModel()
        {
            ToDoItems = new ObservableCollection<TaskModel>();

            AddCommand = new Command(ToDoAdd, CantoDoAdd);
            RemoveCommand = new Command(ToDoRemove);
        }

        #region AddCommand
        public Command AddCommand { get; set; }

        public bool CantoDoAdd()
            =>!string.IsNullOrWhiteSpace(ToDoItemText);

        private void ToDoAdd(object obj)
        {
            var newItem = new TaskModel { Name = ToDoItemText.Trim(), IsChecked = false };

            if (_itemsData.Add(newItem))
            {
                ToDoItems.Add(newItem);
                ToDoItemText = "";
            }
            else
            {
                ShowTemporalMessageAsync(MESSAGE_DUPLICATION);
            }
        }
        #endregion

        private void ToDoRemove(object obj)
        {
            var selectedItems = (IList)obj;

            var n = selectedItems.Count;

            for (int i = n - 1; i >= 0; i--)
                if (_itemsData.Remove((TaskModel)selectedItems[i]))
                    ToDoItems.Remove((TaskModel)selectedItems[i]);
        }

        private async void ShowTemporalMessageAsync(string message)
        {
            NotificationMessage = message;
            await Task.Delay(new TimeSpan(0, 0, 5));
            NotificationMessage = MESSAGE_CLEAR;
        }
    }
}
