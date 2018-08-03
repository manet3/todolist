using CSharpFunctionalExtensions;
using System.Collections.Generic;
using System.Linq;
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

        public void Add(ToDoItem item)
            => _failedActions.Enqueue(new ItemSendAction(item, ApiAction.Add));

        public void DeleteByName(ToDoItem item)
            => _failedActions.Enqueue(new ItemSendAction(item, ApiAction.Delete));

        public void Update(ToDoItem item)
            => _failedActions.Enqueue(new ItemSendAction(item, ApiAction.Change));

        public async Task<Result<IEnumerable<ToDoItem>, RequestError>> GetWhenSynchronisedAsync()
        {
            var queRes = await FinishQueue();
            if (queRes.IsFailure)
                return Result.Fail<IEnumerable<ToDoItem>, RequestError>(queRes.Error);

            return await _req.GetTasksAsync();
        }

        private async Task<Result<object, RequestError>> FinishQueue()
        {
            while (_failedActions.Any())
            {
                var firstAction = _failedActions.Peek();
                var res = await _req.SendRequestAsync(firstAction.Item, firstAction.Action);

                if (res.IsFailure)
                {
                    if (res.Error.ErrorType == RequestErrorType.ServerError)
                        _failedActions.Dequeue();
                    return res;
                }

                _failedActions.Dequeue();
            }
            return Result.Ok<object, RequestError>(new { });
        }

        public SyncMemento Save()
            => new SyncMemento(_failedActions);

        public void Restore(SyncMemento memento)
            => _failedActions = memento.Actions;
    }
}
