using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToDoList.Client.DataServices;
using ToDoList.Client.Tests.Mock;
using System.Collections.Generic;
using FluentAssertions;
using ToDoList.Shared;
using System.Linq;
using System;
using System.Reflection;

namespace ToDoList.Client.Tests
{
    [TestClass]
    public class SyncTests
    {
        private Sync _sync;

        private RequestSenderMock _serverMock;

        private ErrorOnAction[] _regularErrors = new[]
            {
                new ErrorOnAction(ApiAction.Add, RequestErrorType.NoConnection),
                new ErrorOnAction(ApiAction.Change, RequestErrorType.Cancelled),
                new ErrorOnAction(ApiAction.Delete, RequestErrorType.Cancelled)
            };

        private ErrorOnAction[] _serverErrors = new[]
            {
                new ErrorOnAction(ApiAction.Add, RequestErrorType.ServerError),
                new ErrorOnAction(ApiAction.Change, RequestErrorType.ServerError)
            };

        [TestInitialize]
        public void MakeSync()
        {
            _serverMock = new RequestSenderMock();
            _sync = new Sync(_serverMock);
        }

        [TestMethod]
        public void CanFinishQueue()
        {
            SendRequests();

            SyncRepete(1);

            ((Queue<ItemSendAction>)_sync.CurrentState).Should().HaveCount(0);
        }

        [TestMethod]
        public void CannotFinishQueueWhenErrors()
        {
            _serverMock.ActionErrors = _regularErrors;
            SendRequests();

            SyncRepete(3);

            ((Queue<ItemSendAction>)_sync.CurrentState).Should().HaveCount(3);
        }

        [TestMethod]
        public void CanFinishQueueWhenServerErrors()
        {
            _serverMock.ActionErrors = _serverErrors;
            SendRequests();

            SyncRepete(2);

            ((Queue<ItemSendAction>)_sync.CurrentState).Should().HaveCount(0);
        }

        [TestMethod]
        public void CanGet()
        {
            ToDoItemsList[] items = new ToDoItemsList[0];
            _sync.GotItems += (gotItems) => items = gotItems.ToArray();
            SendRequests();

            SyncRepete(3);

            items.Should().HaveCount(10);
        }

        [TestMethod]
        public void CanGetWhenServerErrors()
        {
            _serverMock.ActionErrors = _serverErrors;
            CanGet();
        }

        [TestMethod]
        public void CanInterpreteServerErrorDurringGet()
        {
            _serverMock.ActionErrors = new[] { new ErrorOnAction(ApiAction.List, RequestErrorType.ServerError) };
            RequestError gotError = default(RequestError);
            _sync.ErrorOccured += (error) => gotError = error;

            _sync.Synchronize();

            gotError.Type.Should().Be(RequestErrorType.NoConnection);
        }

        [TestMethod]
        public void CanCallWhenErrors()
            => ListErrorCalls(_regularErrors).Should()
            .Equal(new RequestErrorType[4].Select(x => _regularErrors[0].Error.Type));

        [TestMethod]
        public void CanCallWhenServerErrors()
            => ListErrorCalls(_serverErrors).Should()
            .Equal(_serverErrors.Select(x => x.Error.Type));

        private IEnumerable<RequestErrorType> ListErrorCalls(ErrorOnAction[] errorsOnAction)
        {
            List<RequestErrorType> errors = new List<RequestErrorType>();
            _serverMock.ActionErrors = errorsOnAction;
            _sync.ErrorOccured += (error) => errors.Add(error.Type);
            SendRequests();

            SyncRepete(3);

            return errors;
        }

        private void SendRequests()
        {
            _sync.Add(new ToDoItem());
            _sync.Update(new ToDoItem());
            _sync.Delete(new ToDoItem());
        }

        private void SyncRepete(int times)
        {
            for (int i = 0; i <= times; i++)
                _sync.Synchronize();
        }

        private Queue<ItemSendAction> GetQueue(ApiAction[] actions)
            => new Queue<ItemSendAction>(
                actions.Select(a => new ItemSendAction(new ToDoItem(), a)));
    }
}
