using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ToDoList.Shared;

namespace ToDoList.Client.ViewModels
{
    class ToDoViewModel : ViewModelBase
    {
        string _toDoItemText;
        public string ToDoItemText
        {
            get => _toDoItemText;
            set
            {
                SetValue(ref _toDoItemText, value);
                AddCommand.RaiseExecuteChanged();
            }
        }

        string _notification;
        public string NotificationMessage
        {
            get => _notification;
            set => SetValue(ref _notification, value);
        }

        private const string MESSAGE_CLEAR = "";
        private const string MESSAGE_DUPLICATION = "Doing the same task twice is unproductive.";

        public ObservableCollection<ToDoItem> ToDoItems { get; }

        private HashSet<ToDoItem> _itemsData = new HashSet<ToDoItem>();

        public ToDoViewModel()
        {
            ToDoItems = new ObservableCollection<ToDoItem>();

            AddCommand = new Command(ToDoAdd, CantoDoAdd);
            RemoveCommand = new Command(ToDoRemove);
        }

        #region AddCommand
        public Command AddCommand { get; set; }

        public bool CantoDoAdd()
            => !string.IsNullOrWhiteSpace(ToDoItemText);

        private void ToDoAdd(object obj)
        {
            var newItem = new ToDoItem { Name = ToDoItemText.Trim(), IsChecked = false };

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

        #region RemoveCommand
        public Command RemoveCommand { get; set; }

        private void ToDoRemove(object obj)
        {
            var selectedItems = (IList)obj;
            var selectedArray = new ToDoItem[selectedItems.Count];

            selectedItems.CopyTo(selectedArray, 0);

            foreach (var item in selectedArray)
                ToDoItems.Remove(item);
        }
        #endregion

        private async void ShowTemporalMessageAsync(string message)
        {
            NotificationMessage = message;
            await Task.Delay(new TimeSpan(0, 0, 5));
            NotificationMessage = MESSAGE_CLEAR;
        }
    }
}
