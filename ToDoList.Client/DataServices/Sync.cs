using System;
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
        event Action LongLoadingStarted;

        object CurrentState { get; }
        void RestoreState(object state);
    }

    public class Sync : ISync
    {
        private const int SYNC_PERIOD_SEC = 5;

        private IRequestSender _req;

        private Queue<ItemSendAction> _failedActions = new Queue<ItemSendAction>();

        private DispatcherTimer _syncTimer;

        public Sync(IRequestSender req)
            => _req = req;

        public event Action<IEnumerable<ToDoItem>> GotItems;

        public event Action<RequestError> ErrorOccured;

        public event Action LongLoadingStarted;

        public void Add(ToDoItem item)
            => _failedActions.Enqueue(new ItemSendAction(item, ApiAction.Add));

        public void Delete(ToDoItem item)
            => _failedActions.Enqueue(new ItemSendAction(item, ApiAction.Delete));

        public void Update(ToDoItem item)
            => _failedActions.Enqueue(new ItemSendAction(item, ApiAction.Change));

        public void StartSync()
        {
            Synchronize();
            SyncTimerInit();
            _syncTimer.Start();
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
            if (!await TryFinishQueue() || !await TryGet())
                _syncTimer.Stop();
        }

        private async Task<bool> TryGet()
        {
            var res = await CheckIfLongLoading(_req.GetTasksAsync());
            if (res.IsFailure)
                ErrorOccured?.Invoke(res.Error);
            else
                GotItems?.Invoke(res.Value);

            return !res.IsFailure;
        }

        private async Task<bool> TryFinishQueue()
        {
            var res = RequestResult.Ok();

            while (_failedActions.Any() && !res.IsFailure)
            {
                var firstAction = _failedActions.Peek();

                res = await CheckIfLongLoading(
                    _req.SendRequestAsync(firstAction.Item, firstAction.Action));

                if (!res.IsFailure || res.Error.Type == RequestErrorType.ServerError)
                    _failedActions.Dequeue();

                if (res.IsFailure)
                    ErrorOccured?.Invoke(res.Error);
            }

            return !res.IsFailure;
        }

        private T CheckIfLongLoading<T>(T task) where T : Task
        {
            if (!task.IsCompleted)
                LongLoadingStarted?.Invoke();
            return task;
        }

        public object CurrentState => _failedActions;

        public void RestoreState(object state)
        {
            if (state is Queue<ItemSendAction> actions)
                _failedActions = new Queue<ItemSendAction>(actions);
        }
    }
}
