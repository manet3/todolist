using System;
using System.Collections.Generic;
using System.Linq;
using ToDoList.Client.DataServices;
using ToDoList.Shared;

namespace ToDoList.Client.Test.Mock
{
    class SyncMock : ISync
    {
        public TimeSpan RequestDelay = TimeSpan.MinValue;

        public Stack<RequestError> ErrorsStack = new Stack<RequestError>();

        public List<ToDoItem> SyncList = new List<ToDoItem>();

        public event Action<IEnumerable<ToDoItemsList>> GotItems;
        public event Action<RequestError> ErrorOccured;
        public event Action LoadingStarted;
        public event Action LoadingSucceeded;

        public void Add(ToDoItem item)
            => SyncList.Add(item);

        public void Delete(ToDoItem item) { }

        public void Update(ToDoItem item) { }

        public void StartSync()
        {
        }

        public object CurrentState => SyncList;

        public void RestoreState(object state)
        {
            if (state is List<ToDoItem> syncList)
                SyncList = syncList;
        }
    }
}
