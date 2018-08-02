using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
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

    class Synchronisation
    {
        private RequestSender _req;

        private Queue<ItemSendAction> _failedActions = new Queue<ItemSendAction>();

        public Synchronisation()
            => _req = RequestSender.SyncInit();

        public async Task<Result<object>> AddAsync(ToDoItem item)
            => await HandleRes(item, ApiAction.Add);

        public async Task<Result<object>> DeleteByNameAsync(ToDoItem item)
            => await HandleRes(item, ApiAction.Delete);

        public async Task<Result<object>> UpdateAsync(ToDoItem item)
            => await HandleRes(item, ApiAction.Change);

        public async Task<Result<IEnumerable<ToDoItem>>> GetWhenSynchronisedAsync()
        {
            var queRes = await FinishQueue();
            if (queRes.IsFailure)
                return Result.Fail<IEnumerable<ToDoItem>>(queRes.Error);

            return await _req.GetTasksAsync();
        }

        private async Task<Result<object>> HandleRes(ToDoItem item, ApiAction actionType)
        {
            var action = new ItemSendAction(item, actionType);

            if (_failedActions.Count != 0)
            {
                _failedActions.Enqueue(action);
                return Result.Fail<object>("Waiting for connection...");
            }

            var res = await _req.SendRequestAsync(item, actionType);

            if (res.IsFailure)
                _failedActions.Enqueue(action);

            return res;
        }

        private async Task<Result> FinishQueue()
        {
            while (_failedActions.Any())
            {
                var firstAction = _failedActions.Peek();
                var res = await _req.SendRequestAsync(firstAction.Item, firstAction.Action);

                if (res.IsFailure)
                    return res;

                _failedActions.Dequeue();
            }
            return Result.Ok();
        }

        public SyncMemento Save()
            => new SyncMemento(_failedActions);

        public void Restore(SyncMemento memento) 
            => _failedActions = memento.Actions;
    }
}
