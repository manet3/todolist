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
    public class ObservableUniqueItemsList : ObservableCollection<ToDoItem>
    {
        private HashSet<ToDoItem> _hashSet;

        public ObservableUniqueItemsList() : base()
            => _hashSet = new HashSet<ToDoItem>();

        public ObservableUniqueItemsList(IEnumerable<ToDoItem> collection)
            : base(collection)
            => _hashSet = new HashSet<ToDoItem>(collection);

        public ObservableUniqueItemsList(List<ToDoItem> collection)
            : this((IEnumerable<ToDoItem>)collection) { }

        public new bool Add(ToDoItem item)
        {
            var isUnique = (_hashSet.Add(item));

            if (isUnique)
                base.Add(item);

            return isUnique;
        }

        public new void Remove(ToDoItem item)
        {
            base.Remove(item);
            _hashSet.Remove(item);
        }
    }

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

        string _temporalNotification;
        public string TemporalNotificationMessage
        {
            get => _temporalNotification;
            set => SetValue(ref _temporalNotification, value);
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
        private const int SYNC_PERIOD_SEC = 5;

        private ObservableUniqueItemsList _toDoItems;
        public ObservableUniqueItemsList ToDoItems
        {
            get => _toDoItems;
            private set => SetValue(ref _toDoItems, value);
        }

        private Sync _sync;

        public ToDoViewModel()
        {
            AddCommand = new Command(ToDoAdd, CanToAdd);
            RemoveCommand = new Command(ToDoRemoveItems);
            ChangeCommand = new Command(SendChangeItem);
            SyncRetryCommand = new Command(obj => RefreshListStart());

            Application.Current.Exit += AppExit;

            ToDoItems = new ObservableUniqueItemsList();
            _sync = new Sync(RequestSender.SyncInit());

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

            if (ToDoItems.Add(newItem))
            {
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

            if (res.IsFailure && res.Error.Type == RequestErrorType.ServerError)
            {
                //User will not see message re-opened if the same one is thrown 
                //(if GET is not working, it will not be canceled)
                NotificationMessage = res.Error.Message;
                await Task.Delay(TimeSpan.FromSeconds(MESSAGE_DELAY_SEC));

                RefreshListStart();
                return;
            }

            if (!res.IsFailure)
                ToDoItems = new ObservableUniqueItemsList(res.Value);
            //stop sync in case of an error
            else return;

            await Task.Delay(TimeSpan.FromSeconds(SYNC_PERIOD_SEC));
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
                ToDoItems = new ObservableUniqueItemsList(session.List);
            }
        }

        private async void ShowTemporalMessage(string message)
        {
            TemporalNotificationMessage = message;
            await Task.Delay(TimeSpan.FromSeconds(MESSAGE_DELAY_SEC));
            TemporalNotificationMessage = MESSAGE_CLEAR;
        }

        private async Task<Result<IEnumerable<ToDoItem>, RequestError>> DisplaySync(Task<Result<IEnumerable<ToDoItem>, RequestError>> task)
        {
            CheckIfDisplay(task);

            var res = await task;

            // show/hide notification, stop loader
            if (res.IsFailure)
            {
                NotificationMessage = res.Error.Message;

                var errorType = res.Error.Type;

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
