using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToDoList.Client.DataServices;
using ToDoList.Client.Tests.Mock;
using System.Collections.Generic;
using FluentAssertions;
using ToDoList.Shared;
using System.Linq;
using System;

namespace ToDoList.Client.Tests
{
    [TestClass]
    public class SyncTests
    {
        private Sync _sync;

        private RequestSenderMock _serverMock;

        [TestInitialize]
        public void MakeSync()
        {
            _serverMock = new RequestSenderMock();
            _sync = new Sync(_serverMock);
        }

        [TestMethod]
        public void CanFinishQueue()
        {
            _sync = new SyncMemento(GetQueue(new[] { ApiAction.Add, ApiAction.Change, ApiAction.Delete })));
            _serverMock.ActionErrors = GetErrored(RequestErrorType.ServerError, ApiAction.Delete, ApiAction.Add);

            Action getWhenSync = async () =>
            {
                RequestErrorType errorType;
                do errorType = (await _sync.GetWhenSynchronisedAsync()).Error.Type;
                while (errorType == RequestErrorType.ServerError);
            };
            getWhenSync();

            ((SyncMemento)_sync.Save()).Actions.Should().HaveCountLessOrEqualTo(0);
        }

        private ActionErrorMock[] GetErrored(RequestErrorType typeOfError, params ApiAction[] toError)
            => toError.Select(a => new ActionErrorMock(a,
                new RequestError(string.Empty, typeOfError)))
                .ToArray();

        private Queue<ItemSendAction> GetQueue(ApiAction[] actions)
            => new Queue<ItemSendAction>(
                actions.Select(a => new ItemSendAction(new ToDoItem(), a)));
    }
}
