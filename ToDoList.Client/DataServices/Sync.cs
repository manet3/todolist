﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using ToDoList.Shared;

namespace ToDoList.Client.DataServices
{
    public struct ItemSendAction
    {
        public ApiAction Action;
        public ToDoItem Item;

        public ItemSendAction(ToDoItem item, ApiAction action)
            => (Action, Item) = (action, item);
    }

    public interface ISync
    {
        void Add(ToDoItem item);
        void Delete(ToDoItem item);
        void Update(ToDoItem item);

        void StartSync();

        event Action<IEnumerable<ToDoItem>> GotItems;
        event Action<RequestError> ErrorOccured;
        event Action LoadingStarted;
        event Action LoadingSucceeded;

        object CurrentState { get; }
        void RestoreState(object state);
    }

    public class Sync : ISync
    {
        private const int SYNC_PERIOD_SEC = 5;

        private IRequestSender _req;

        private Queue<ItemSendAction> _syncActions = new Queue<ItemSendAction>();

        private DispatcherTimer _syncTimer;

        public Sync(IRequestSender req)
            => _req = req;

        public event Action<IEnumerable<ToDoItem>> GotItems;

        public event Action<RequestError> ErrorOccured;

        public event Action LoadingStarted;

        public event Action LoadingSucceeded;

        public void Add(ToDoItem item)
            => _syncActions.Enqueue(new ItemSendAction(item, ApiAction.Add));

        public void Delete(ToDoItem item)
            => _syncActions.Enqueue(new ItemSendAction(item, ApiAction.Delete));

        public void Update(ToDoItem item)
            => _syncActions.Enqueue(new ItemSendAction(item, ApiAction.Change));

        public void StartSync()
        {
            SyncTimerInit();
            Synchronize();
        }

        private void SyncTimerInit()
        {
            _syncTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(SYNC_PERIOD_SEC)
            };
            _syncTimer.Tick += (s, e) => Synchronize();
        }

        private async void Synchronize()
        {
            LoadingStarted?.Invoke();
            _syncTimer?.Stop();

            var isSyncSuccessful = await TryFinishQueue();

            if (isSyncSuccessful)
                isSyncSuccessful = await TryGet();

            if (isSyncSuccessful)
            {
                LoadingSucceeded?.Invoke();
                _syncTimer?.Start();
            }
        }

        private async Task<bool> TryFinishQueue()
        {
            var res = RequestResult.Ok();

            while (_syncActions.Any() && !res.IsFailure)
            {
                var firstAction = _syncActions.Peek();

                res = await _req.SendRequestAsync(firstAction.Item, firstAction.Action);

                if (!res.IsFailure || res.Error.Type == RequestErrorType.ServerError)
                    _syncActions.Dequeue();

                if (res.IsFailure)
                    ErrorOccured?.Invoke(res.Error);
            }

            return !res.IsFailure;
        }

        private async Task<bool> TryGet()
        {
            var res = await _req.GetTasksAsync();
            if (res.IsFailure)
                ErrorOccured?.Invoke(res.Error);
            else
                GotItems?.Invoke(res.Value);

            return !res.IsFailure;
        }

        public object CurrentState => _syncActions;

        public void RestoreState(object state)
        {
            if (state is Queue<ItemSendAction> actions)
                _syncActions = new Queue<ItemSendAction>(actions);
        }
    }
}