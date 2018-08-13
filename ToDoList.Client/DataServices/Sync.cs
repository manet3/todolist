﻿using System.Collections.Generic;
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

    public interface ISync
    {
        void Add(ToDoItem item);
        void Delete(ToDoItem item);
        void Update(ToDoItem item);

        Task<RequestResult<IEnumerable<ToDoItem>>> GetWhenSynchronisedAsync();

        object GetState();

        void RestoreState(object state);
    }

    public class Sync : ISync
    {
        private IRequestSender _req;

        private Queue<ItemSendAction> _failedActions = new Queue<ItemSendAction>();

        public Sync(IRequestSender req)
            => _req = req;

        public void Add(ToDoItem item)
            => _failedActions.Enqueue(new ItemSendAction(item, ApiAction.Add));

        public void Delete(ToDoItem item)
            => _failedActions.Enqueue(new ItemSendAction(item, ApiAction.Delete));

        public void Update(ToDoItem item)
            => _failedActions.Enqueue(new ItemSendAction(item, ApiAction.Change));

        public async Task<RequestResult<IEnumerable<ToDoItem>>> GetWhenSynchronisedAsync()
        {
            var queRes = await FinishQueue();
            if (queRes.IsFailure)
                return RequestResult.Fail<IEnumerable<ToDoItem>>(queRes.Error);

            return await _req.GetTasksAsync();
        }

        private async Task<RequestResult> FinishQueue()
        {
            while (_failedActions.Any())
            {
                var firstAction = _failedActions.Peek();
                var res = await _req.SendRequestAsync(firstAction.Item, firstAction.Action);

                if (res.IsFailure)
                {
                    if (res.Error.Type == RequestErrorType.ServerError)
                        _failedActions.Dequeue();
                    return res;
                }

                _failedActions.Dequeue();
            }
            return RequestResult.Ok();
        }

        public void RestoreState(object state)
        {
            if (state is Queue<ItemSendAction> actions)
                _failedActions = new Queue<ItemSendAction>(actions);
        }

        public object GetState() => _failedActions;
    }
}
