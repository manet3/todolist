using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ToDoList.Shared;
using ToDoList.Client.DataServices;
using ToDoList.Client.ViewModels.Common;
using ToDoList.Client.Controls;

namespace ToDoList.Client.ViewModels
{
    public class ObservableHashSet : ObservableCollection<ToDoItem>
    {
        private HashSet<ToDoItem> _hashSet;

        public ObservableHashSet() : base()
            => _hashSet = new HashSet<ToDoItem>();

        public ObservableHashSet(IEnumerable<ToDoItem> collection)
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

        private ObservableHashSet _toDoItems;
        public ObservableHashSet ToDoItems
        {
            get => _toDoItems;
            private set => SetValue(ref _toDoItems, value);
        }

        private ISync _sync;

        public ToDoViewModel(ISync sync)
        {
            StartCommand = new Command(obj => OnStart());
            ClosingCommand = new Command(obj => OnClosing());
            AddCommand = new Command(obj => ToDoAdd(), CanToAdd);
            RemoveCommand = new Command(ToDoRemoveItems);
            ChangeCommand = new Command(SendChangeItem);
            SyncRetryCommand = new Command(obj => _sync.StartSync());

            ToDoItems = new ObservableHashSet();

            _sync = sync;
            SubscribeOnSyncEvents();
        }

        #region Start
        public Command StartCommand { get; set; }

        private void OnStart()
        {
            GetSavedSession();
            _sync.StartSync();
        }
        #endregion

        private void GetSavedSession()
        {
            var session = SessionSaver.FromJson();
            if (session != null)
            {
                _sync.RestoreState(session.SyncState);
                ToDoItems = new ObservableHashSet(session.List);
            }
        }

        #region Closing
        public Command ClosingCommand { get; set; }

        private void OnClosing()
            => new SessionSaver(_sync.CurrentState, ToDoItems).SaveJson();
        #endregion

        public Command SyncRetryCommand { get; set; }

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

        private void SubscribeOnSyncEvents()
        {
            _sync.GotItems += (items) => ToDoItems = new ObservableHashSet(items);          

            _sync.LoadingSucceeded += OnLoadingSucceeded;

            _sync.ErrorOccured += OnLoadingFailed;

            _sync.LoadingStarted += () => LoaderState = LoadingState.Started;
        }

        private void OnLoadingSucceeded()
        {
            FinishLoader(RequestErrorType.None);
            ErrorMessage = string.Empty;
        }

        private void OnLoadingFailed(RequestError error)
        {
            FinishLoader(error.Type);
            ErrorMessage = error.Message;
        }

        private void FinishLoader(RequestErrorType errorType)
        {
            switch (errorType)
            {
                case RequestErrorType.None:
                    LoaderState = LoadingState.None; break;
                case RequestErrorType.NoConnection:
                    LoaderState = LoadingState.Failed; break;
                case RequestErrorType.Cancelled:
                    LoaderState = LoadingState.Paused; break;
            }
        }
    }
}
