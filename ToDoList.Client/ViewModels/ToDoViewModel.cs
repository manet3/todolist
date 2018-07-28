using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ToDoList.Shared;
using ToDoList.Client.DataServices;
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

        private DataServicesManager _dataManager;

        public ToDoViewModel()
        {
            ToDoItems = new ObservableCollection<ToDoItem>();
            _dataManager = new DataServicesManager();

            AddCommand = new Command(ToDoAdd, CanToAdd);
            RemoveCommand = new Command(ToDoRemoveItems);
            ChangeCommand = new Command(SendChangeItem);
            FinishingCommand = new Command(SaveOnFinishing);
            SyncRetryCommand = new Command(SyncRetry);

            GetListAsync();
        }

        private async void GetListAsync()
        {
            var res = await DisplaySync(_dataManager.GetAsync());
            if (res != null)
                ToDoItems = new ObservableCollection<ToDoItem>(res);
        }

        #region Restart
        public Command SyncRetryCommand { get; set; }

        private async void SyncRetry(object obj)
        {
            if (ToDoItems.Count == 0)
                GetListAsync();
            else
                await DisplaySync(_dataManager.UpdateAllAsync(ToDoItems));
        }
        #endregion

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

            await DisplaySync(_dataManager.AddAsync(newItem));
        }
        #endregion

        #region Change item
        public Command ChangeCommand { get; set; }

        private async void SendChangeItem(object obj)
            => await DisplaySync(_dataManager.UpdateAsync((ToDoItem)obj));
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
                await DisplaySync(_dataManager.DeleteByNameAsync(item.Name));
                if (LoaderState == LoadingState.Failed)
                    return;
            }
        }
        #endregion

        #region Finishing
        public Command FinishingCommand { get; set; }

        private void SaveOnFinishing(object obj)
            => _dataManager.SaveIfNotSynchronised();
        #endregion

        private async Task<T> DisplaySync<T>(Task<Result<T>> task)
        {
            if (!task.IsCompleted)
                LoaderState = LoadingState.Started;

            var res = await task;

            if (res.IsFailure)
            {
                (NotificationMessage, LoaderState) = (res.Error, LoadingState.Failed);
                return default;
            }
            (NotificationMessage, LoaderState) = (string.Empty, LoadingState.None);
            return res.Value;
        }

        private async void ShowTemporalMessageAsync(string message)
        {
            NotificationMessage = message;
            await Task.Delay(new TimeSpan(0, 0, 5));
            NotificationMessage = MESSAGE_CLEAR;
        }


    }
}
