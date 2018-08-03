using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ToDoList.Shared;
using ToDoList.Client.DataServices;
using ToDoList.Client.Controls;
using System.Windows;
using CSharpFunctionalExtensions;

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
        //here 0 means 'infinity'
        private const int MESSAGE_DELAY_SEC = 5;
        private const int UNDISPLAIED_LOADING_SEC = 3;


        private ObservableCollection<ToDoItem> _toDoItems;
        public ObservableCollection<ToDoItem> ToDoItems
        {
            get => _toDoItems;
            private set => SetValue(ref _toDoItems, value);
        }

        private HashSet<ToDoItem> _itemsData;

        private Synchronisation _sync;

        public ToDoViewModel()
        {
            AddCommand = new Command(ToDoAdd, CanToAdd);
            RemoveCommand = new Command(ToDoRemoveItems);
            ChangeCommand = new Command(SendChangeItem);
            SyncRetryCommand = new Command(obj => RefreshListStart());

            Application.Current.Exit += AppExit;

            ToDoItems = new ObservableCollection<ToDoItem>();
            _itemsData = new HashSet<ToDoItem>();
            _sync = new Synchronisation();

            GetSavedSession();
            RefreshListStart();
        }

        #region Add item
        public Command AddCommand { get; set; }

        public bool CanToAdd()
            => !string.IsNullOrWhiteSpace(ToDoItemText);

        private void ToDoAdd(object obj)
        {
            var newItem = new ToDoItem { Name = ToDoItemText.Trim(), IsChecked = false };

            if (_itemsData.Add(newItem))
            {
                ToDoItems.Add(newItem);
                ToDoItemText = "";
                _sync.Add(newItem);
            }
            else
            {
                ShowTemporalMessage(MESSAGE_DUPLICATION);
            }
        }
        #endregion

        #region Change item
        public Command ChangeCommand { get; set; }

        private void SendChangeItem(object obj)
            => _sync.Update((ToDoItem)obj);
        #endregion

        #region Remove items
        public Command RemoveCommand { get; set; }

        private void ToDoRemoveItems(object obj)
        {
            var selectedItems = (IList)obj;
            var selectedArray = new ToDoItem[selectedItems.Count];

            selectedItems.CopyTo(selectedArray, 0);

            foreach (var item in selectedArray)
            {
                ToDoItems.Remove(item);
                _sync.DeleteByName(item);
            }
        }
        #endregion

        public Command SyncRetryCommand { get; set; }

        private async void RefreshListStart()
        {
            Result<IEnumerable<ToDoItem>, RequestError> res = await DisplaySync(_sync.GetWhenSynchronisedAsync());

            while (res.IsFailure && res.Error.ErrorType == RequestErrorType.ServerError)
            {
                await ShowDelaiedMessage(res.Error.Message);
                RefreshListStart();
                return;
            }

            if (!res.IsFailure)
                ToDoItems = new ObservableCollection<ToDoItem>(res.Value);
            //stop sync in case of an error
            else return;

            RefreshListStart();
        }

        private void AppExit(object sender, ExitEventArgs e)
            => new SavedSession(_sync.Save(), ToDoItems).SaveJson();

        private void GetSavedSession()
        {
            var session = SavedSession.FromJson();
            if (session != null)
            {
                _sync.Restore(session.SyncState);
                ToDoItems = new ObservableCollection<ToDoItem>(session.List);
            }
        }

        private async void ShowTemporalMessage(string message)
        {
            await ShowDelaiedMessage(message);
            NotificationMessage = MESSAGE_CLEAR;
        }

        private async Task ShowDelaiedMessage(string message)
        {
            NotificationMessage = message;
            await Task.Delay(TimeSpan.FromSeconds(MESSAGE_DELAY_SEC));
        }

        private async Task<Result<IEnumerable<ToDoItem>, RequestError>> DisplaySync(Task<Result<IEnumerable<ToDoItem>, RequestError>> task)
        {
            CheckIfDisplay(task);

            var res = await task;

            // show/hide notification, stop loader
            if (res.IsFailure)
            {
                NotificationMessage = res.Error.Message;

                var errorType = res.Error.ErrorType;

                switch (errorType)
                {
                    case RequestErrorType.NoConnection:
                        LoaderState = LoadingState.Failed; break;
                    case RequestErrorType.Cancelled:
                        LoaderState = LoadingState.Paused; break;
                }
            }
            else
                (NotificationMessage, LoaderState) = (string.Empty, LoadingState.None);

            return res;
        }

        private async void CheckIfDisplay(Task task)
        {
            if (task.IsCompleted) return;

            await Task.Delay(TimeSpan.FromSeconds(UNDISPLAIED_LOADING_SEC));

            if (!task.IsCompleted)
                LoaderState = LoadingState.Started;
        }
    }
}
