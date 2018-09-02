using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using ToDoList.Shared;
using ToDoList.Client.DataServices;
using ToDoList.Client.ViewModels.Common;
using ToDoList.Client.Controls;
using System.Linq;

namespace ToDoList.Client.ViewModels
{
    public class InteractiveToDoList : ToDoItemsList
    {
        public new ObservableHashSet<ToDoItem> Items { get; set; }

        public InteractiveToDoList() { }

        public InteractiveToDoList(ToDoItemsList baseList)
        {
            Id = baseList.Id;
            Name = baseList.Name;
            Timestamp = baseList.Timestamp;
            Items = new ObservableHashSet<ToDoItem>(baseList.Items);
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

        private ObservableHashSet<InteractiveToDoList> _toDoLists;
        public ObservableHashSet<InteractiveToDoList> ToDoLists
        {
            get => _toDoLists;
            private set => SetValue(ref _toDoLists, value);
        }

        public InteractiveToDoList ActiveToDoList { get; set; }

        private ISync _sync;

        public ToDoViewModel(ISync sync)
        {
            StartCommand = new Command(obj => OnStart());
            ClosingCommand = new Command(obj => OnClosing());
            AddCommand = new Command(obj => ToDoAdd(), CanToAdd);
            RemoveCommand = new Command(ToDoRemoveItems);
            ChangeCommand = new Command(SendChangeItem);
            SyncRetryCommand = new Command(obj => _sync.StartSync());

            ToDoLists = new ObservableHashSet<InteractiveToDoList> {
                new InteractiveToDoList { Name = "ToDolist 1",
                    Items = new ObservableHashSet<ToDoItem>( 
                        new[]{ new ToDoItem { Name = "Item 1", IsChecked = true} }) } };

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
                ToDoLists = MakeListsInteractive(session.Lists);
            }
        }

        #region Closing
        public Command ClosingCommand { get; set; }

        private void OnClosing()
            => new SessionSaver(_sync.CurrentState, ToDoLists).SaveJson();
        #endregion

        public Command SyncRetryCommand { get; set; }

        #region Add item
        public Command AddCommand { get; set; }

        public bool CanToAdd()
            => !string.IsNullOrWhiteSpace(NewItemText);

        private void ToDoAdd()
        {
            var newItem = new ToDoItem { Name = NewItemText.Trim(), IsChecked = false };

            if (ActiveToDoList.Items.Add(newItem))
            {
                NewItemText = "";
                newItem.UpdateTimestamp();
                _sync.Add(newItem);
            }
            else
            {
                ShowTemporalMessage(MESSAGE_DUPLICATION);
            }
        }
        #endregion

        private async void ShowTemporalMessage(string message)
            => await ShowTemporalMessageAsync(message);

        private async Task ShowTemporalMessageAsync(string message)
        {
            TemporalNotificationMessage = message;
            await Task.Delay(TimeSpan.FromSeconds(MESSAGE_DELAY_SEC));
            TemporalNotificationMessage = string.Empty;
        }

        #region Change item
        public Command ChangeCommand { get; set; }

        private void SendChangeItem(object obj)
        {
            var item = (ToDoItem)obj;
            item.UpdateTimestamp();
            _sync.Update(item);
        }
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
                ActiveToDoList.Items.Remove(item);
                item.UpdateTimestamp();
                _sync.Delete(item);
            }
        }
        #endregion

        private void SubscribeOnSyncEvents()
        {
            _sync.GotItems += (lists) => ToDoLists = MakeListsInteractive(lists);

            _sync.LoadingSucceeded += OnLoadingSucceeded;

            _sync.ErrorOccured += OnLoadingFailed;

            _sync.LoadingStarted += () => LoaderState = LoadingState.Started;
        }

        private ObservableHashSet<InteractiveToDoList> MakeListsInteractive(IEnumerable<ToDoItemsList> lists)
            => new ObservableHashSet<InteractiveToDoList>(lists.Select(x => new InteractiveToDoList(x)));

        private void OnLoadingSucceeded()
        {
            FinishLoader(RequestErrorType.None);
            ErrorMessage = string.Empty;
        }

        private void OnLoadingFailed(RequestError error)
        {
            if (error.Type == RequestErrorType.ServerError)
                OnServerError(error);
            else
            {
                FinishLoader(error.Type);
                ErrorMessage = error.Message;
            }
        }

        private async void OnServerError(RequestError error)
        {
            await ShowTemporalMessageAsync(error.Message);
            _sync.StartSync();
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
