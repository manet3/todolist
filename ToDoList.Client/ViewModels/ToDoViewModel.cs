using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ToDoList.Shared;
using ToDoList.Client.Models;
using CSharpFunctionalExtensions;
using ToDoList.Client.Controls;

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

        private LoadingState _loaderState;
        public LoadingState LoaderState
        {
            get => _loaderState;
            set => SetValue(ref _loaderState, value);
        }

        private const string MESSAGE_CLEAR = "";
        private const string MESSAGE_DUPLICATION = "Doing the same task twice is unproductive.";

        private ObservableCollection<ToDoItem> _toDoItems;
        public ObservableCollection<ToDoItem> ToDoItems
        {
            get => _toDoItems;
            private set => SetValue(ref _toDoItems, value);
        }

        private HashSet<ToDoItem> _itemsData = new HashSet<ToDoItem>();

        private ToDoModel _model;

        public ToDoViewModel()
        {
            ToDoItems = new ObservableCollection<ToDoItem>();
            _model = new ToDoModel();

            AddCommand = new Command(ToDoAdd, CanToAdd);
            RemoveCommand = new Command(ToDoRemoveItems);
            ChangeCommand = new Command(SendChangeItem);
            FinishingCommand = new Command(SaveOnFinishing);

            GetListAsync();
        }

        private async void GetListAsync()
        {
            var resTask = _model.GetAsync();

            if (!resTask.IsCompleted)
                LoaderState = LoadingState.Started;

            var res = await resTask;

            ShowResMessage(res);

            if (res.IsSuccess)
                ToDoItems = new ObservableCollection<ToDoItem>(res.Value);
        }

        #region Add item
        public Command AddCommand { get; set; }

        public bool CanToAdd()
            => !string.IsNullOrWhiteSpace(ToDoItemText);

        private async void ToDoAdd(object obj)
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

            ShowResMessage(await _model.AddAsync(newItem));
        }
        #endregion

        #region Change item
        public Command ChangeCommand { get; set; }

        private async void SendChangeItem(object obj)
            => ShowResMessage(await _model.UpdateAsync((ToDoItem)obj));
        #endregion

        #region Remove items
        public Command RemoveCommand { get; set; }

        private async void ToDoRemoveItems(object obj)
        {
            var selectedItems = (IList)obj;
            var selectedArray = new ToDoItem[selectedItems.Count];

            selectedItems.CopyTo(selectedArray, 0);

            foreach (var item in selectedArray)
                ToDoItems.Remove(item);

            await SendRemoveItemsAsync(selectedArray);
        }

        private async Task SendRemoveItemsAsync(ToDoItem[] items)
        {
            foreach (var item in items)
            {
                var res = await _model.DeleteAsync(item);
                if (res.IsFailure)
                {
                    NotificationMessage = res.Error;
                    return;
                }
                NotificationMessage = string.Empty;
            }
        }
        #endregion

        #region Finishing
        public Command FinishingCommand { get; set; }

        private void SaveOnFinishing(object obj)
            => _model.SaveIfNotSynchronised();
        #endregion

        private void ShowResMessage(Result res)
            => (NotificationMessage, LoaderState) = res.IsFailure
                ? (res.Error, LoadingState.Failed)
                : (string.Empty, LoadingState.None);

        private async void ShowTemporalMessageAsync(string message)
        {
            NotificationMessage = message;
            await Task.Delay(new TimeSpan(0, 0, 5));
            NotificationMessage = MESSAGE_CLEAR;
        }


    }
}
