using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ToDoList.Shared;
using ToDoList.Client.DataServices;
using ToDoList.Client.Controls;
using System.Windows;

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

        private ISync _sync;

        public ToDoViewModel(ISync sync)
        {
            AddCommand = new Command(ToDoAdd, CanToAdd);
            RemoveCommand = new Command(ToDoRemoveItems);
            ChangeCommand = new Command(SendChangeItem);
            SyncRetryCommand = new Command(obj => RefreshListStart());

            Application.Current.Exit += AppExit;

            ToDoItems = new ObservableUniqueItemsList();
            _sync = sync;

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

        private async void ShowTemporalMessage(string message)
        {
            TemporalNotificationMessage = message;
            await Task.Delay(TimeSpan.FromSeconds(MESSAGE_DELAY_SEC));
            TemporalNotificationMessage = string.Empty;
        }

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
                _sync.Delete(item);
            }
        }
        #endregion

        public Command SyncRetryCommand { get; set; }

        private async void RefreshListStart()
        {
            var res = await HandleSyncState(_sync.GetWhenSynchronisedAsync());

            if (res.IsFailure)
                return;

            ToDoItems = new ObservableUniqueItemsList(res.Value);

            await RefreshAfterDelay();
        }

        private async Task RefreshAfterDelay()
        {
            await Task.Delay(TimeSpan.FromSeconds(SYNC_PERIOD_SEC));
            RefreshListStart();
        }

        private async Task<RequestResult<IEnumerable<ToDoItem>>> HandleSyncState(Task<RequestResult<IEnumerable<ToDoItem>>> task)
        {
            CheckStartLoader(task);

            var res = await task;

            FinishLoader(res.Error);
            UpdateErrorMessage(res.Error);

            if (res.Error.Type == RequestErrorType.ServerError)
                await RefreshAfterDelay();

            return res;
        }

        private async void CheckStartLoader(Task task)
        {
            if (task.IsCompleted) return;

            await Task.Delay(TimeSpan.FromSeconds(UNDISPLAIED_LOADING_SEC));

            if (!task.IsCompleted)
                LoaderState = LoadingState.Started;
        }

        private void FinishLoader(RequestError error)
        {
            switch (error.Type)
            {
                case RequestErrorType.None:
                    LoaderState = LoadingState.None; break;
                case RequestErrorType.NoConnection:
                    LoaderState = LoadingState.Failed; break;
                case RequestErrorType.Cancelled:
                    LoaderState = LoadingState.Paused; break;
            }
        }

        private void UpdateErrorMessage(RequestError error)
        {
            if (error.Type == RequestErrorType.None)
                NotificationMessage = string.Empty;
            else NotificationMessage = error.Message;
        }

        private void AppExit(object sender, ExitEventArgs e)
            => new SessionSaver(_sync.Save(), _sync.MementoType, ToDoItems).SaveJson();

        private void GetSavedSession()
        {
            var session = SessionSaver.FromJson(_sync.MementoType);
            if (session != null)
            {
                _sync.Restore(session.SyncMemento);
                ToDoItems = new ObservableUniqueItemsList(session.List);
            }
        }
    }
}
