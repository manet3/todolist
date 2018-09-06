using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using ToDoList.Shared;
using ToDoList.Client.DataServices;
using ToDoList.Client.ViewModels.Common;
using ToDoList.Client.Controls;
using System.Linq;
using ToDoList.Client.ViewModels.Modules;

namespace ToDoList.Client.ViewModels
{
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

        private ObservableHashSet<ObservableToDoItemsList> _toDoLists;
        public ObservableHashSet<ObservableToDoItemsList> ToDoLists
        {
            get => _toDoLists;
            set => SetValue(ref _toDoLists, value);
        }

        private ObservableToDoItemsList _activeToDoList;
        public ObservableToDoItemsList ActiveToDoList
        {
            get => _activeToDoList;
            set
            {
                SetValue(ref _activeToDoList, value);
                AddCommand.RaiseExecuteChanged();
            }
        }

        private ISync _sync;

        public ToDoViewModel(ISync sync)
        {
            StartCommand = new Command(obj => OnStart());
            ClosingCommand = new Command(obj => OnClosing());
            AddCommand = new Command(obj => AddItem(), CanToAdd);
            AddListCommand = new Command(obj => AddList());
            RemoveCommand = new Command(ToDoRemoveItems);
            ChangeCommand = new Command(SendChangeItem);
            SyncRetryCommand = new Command(obj => _sync.StartSync());

            ToDoLists = new ObservableHashSet<ObservableToDoItemsList>();

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

        private void GetSavedSession()
        {
            var session = SessionSaver.FromJson();
            if (session != null)
            {
                _sync.RestoreState(session.SyncState);
                ToDoLists = MakeListsObservable(session.ToDoLists);
            }
        }
        #endregion

        #region Closing
        public Command ClosingCommand { get; set; }

        private void OnClosing()
            => new SessionSaver { SyncState = _sync.CurrentState, ToDoLists = ToDoLists.Cast<ToDoItemsList>().ToList() }.SaveJson();
        #endregion

        public Command SyncRetryCommand { get; set; }

        #region Add item
        public Command AddCommand { get; set; }

        public bool CanToAdd()
            => !string.IsNullOrWhiteSpace(NewItemText) && ActiveToDoList != null;

        private void AddItem()
        {
            var newItem = new ToDoItem { Name = NewItemText.Trim(), IsChecked = false };

            if (ActiveToDoList.Add(newItem))
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
            foreach (var item in ActiveToDoList.SelectedItems)
            {
                ActiveToDoList.Remove(item);
                item.UpdateTimestamp();
                _sync.Delete(item);
            }
        }
        #endregion

        #region Add list
        public Command AddListCommand { get; set; }

        private void AddList()
        {
            var startName = "New list";
            var counter = 0;
            while (!ToDoLists.Add(new ObservableToDoItemsList(new ToDoItemsList
            {
                Name = startName + (counter == 0 ? "" : " " + counter.ToString())
            })))
                counter++;

            ActiveToDoList = ToDoLists.Last();
        }
        #endregion

        private void SubscribeOnSyncEvents()
        {
            _sync.GotItems += (lists) => ToDoLists = MakeListsObservable(lists);

            _sync.LoadingSucceeded += OnLoadingSucceeded;

            _sync.ErrorOccured += OnLoadingFailed;

            _sync.LoadingStarted += () => LoaderState = LoadingState.Started;
        }

        private ObservableHashSet<ObservableToDoItemsList> MakeListsObservable(IEnumerable<ToDoItemsList> lists)
            => new ObservableHashSet<ObservableToDoItemsList>(lists.Select(x => new ObservableToDoItemsList(x)));

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

        private async Task ShowTemporalMessageAsync(string message)
        {
            TemporalNotificationMessage = message;
            await Task.Delay(TimeSpan.FromSeconds(MESSAGE_DELAY_SEC));
            TemporalNotificationMessage = string.Empty;
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
