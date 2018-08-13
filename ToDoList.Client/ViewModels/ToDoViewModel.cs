using System;
using System.Windows;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ToDoList.Shared;
using ToDoList.Client.DataServices;
using ToDoList.Client.ViewModels.Common;
using ToDoList.Client.Controls;
using System.Windows.Threading;

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

        public new bool Add(ToDoItem item)
        {
            var isUnique = _hashSet.Add(item);

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

    public class ToDoViewModel : ViewModelBase
    {
        string _newItemText;
        public string NewItemText
        {
            get => _newItemText;
            set
            {
                SetValue(ref _newItemText, value);
                AddCommand.RaiseExecuteChanged();
            }
        }

        string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetValue(ref _errorMessage, value);
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

        private DispatcherTimer _syncTimer;

        private ISync _sync;

        public ToDoViewModel(ISync sync)
        {
            StartCommand = new Command(obj => OnStart());
            ClosingCommand = new Command(obj => OnClosing());
            AddCommand = new Command(obj => ToDoAdd(), CanToAdd);
            RemoveCommand = new Command(ToDoRemoveItems);
            ChangeCommand = new Command(SendChangeItem);
            SyncRetryCommand = new Command(obj => RefreshListStart());

            SyncTimerInit();

            ToDoItems = new ObservableUniqueItemsList();
            _sync = sync;
        }

        private void SyncTimerInit()
        {
            _syncTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(SYNC_PERIOD_SEC)
            };
            _syncTimer.Tick += (s, e) => RefreshListStart();
        }

        #region Start
        public Command StartCommand { get; set; }

        private void OnStart()
        {
            GetSavedSession();
            RefreshListStart();
        }
        #endregion

        private void GetSavedSession()
        {
            var session = SessionSaver.FromJson();
            if (session != null)
            {
                _sync.RestoreState(session.SyncState);
                ToDoItems = new ObservableUniqueItemsList(session.List);
            }
        }

        #region Closing
        public Command ClosingCommand { get; set; }

        private void OnClosing()
            => new SessionSaver(_sync.GetState(), ToDoItems).SaveJson();
        #endregion

        #region Add item
        public Command AddCommand { get; set; }

        public bool CanToAdd()
            => !string.IsNullOrWhiteSpace(NewItemText);

        private void ToDoAdd()
        {
            var newItem = new ToDoItem { Name = NewItemText.Trim(), IsChecked = false };

            if (ToDoItems.Add(newItem))
            {
                NewItemText = "";
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
            var res = await DisplaySyncState(_sync.GetWhenSynchronisedAsync());

            if (!res.IsFailure)
                ToDoItems = new ObservableUniqueItemsList(res.Value);

            if (res.IsFailure && res.Error.Type != RequestErrorType.ServerError)
                _syncTimer.Stop();
            else if (!_syncTimer.IsEnabled) _syncTimer.Start();
        }

        private async Task RefreshAfterDelay()
        {
            await Task.Delay(TimeSpan.FromSeconds(SYNC_PERIOD_SEC));
            RefreshListStart();
        }

        private async Task<RequestResult<IEnumerable<ToDoItem>>> DisplaySyncState(Task<RequestResult<IEnumerable<ToDoItem>>> task)
        {
            CheckStartLoader(task);

            var res = await task;

            FinishLoader(res.Error);

            UpdateErrorMessage(res.Error);

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
                ErrorMessage = string.Empty;
            else
                ErrorMessage = error.Message;
        }
    }
}
