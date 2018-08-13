using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ToDoList.Client.DataServices;
using ToDoList.Shared;

namespace ToDoList.Client.Test.Mock
{
    class SyncMock : ISync
    {
        public Type MementoType => typeof(object);

        public TimeSpan RequestDelay = TimeSpan.MinValue;

        public Stack<RequestError> ErrorsStack = new Stack<RequestError>();

        public List<ToDoItem> SyncList = new List<ToDoItem>();

        public void Add(ToDoItem item)
            => SyncList.Add(item);

        public void Delete(ToDoItem item)
            => SyncList.Remove(item);

        public void Update(ToDoItem item) { }

        public async Task<RequestResult<IEnumerable<ToDoItem>>> GetWhenSynchronisedAsync()
        {
            if (RequestDelay != TimeSpan.MinValue)
                await Task.Delay(RequestDelay);

            if (ErrorsStack.Any())
                return RequestResult.Fail<IEnumerable<ToDoItem>>(ErrorsStack.Pop());

            return RequestResult.Ok<IEnumerable<ToDoItem>>(SyncList);
        }

        public void Restore(object memento){}

        public object Save() => this;
    }
}
