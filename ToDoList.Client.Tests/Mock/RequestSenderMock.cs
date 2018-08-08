using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using ToDoList.Client.DataServices;
using ToDoList.Shared;
using System;

namespace ToDoList.Client.Tests.Mock
{
    public class ActionErrorMock
    {
        public ApiAction Action;

        public RequestError Error;

        public ActionErrorMock(ApiAction action, RequestError error)
            => (Action, Error) = (action, error);
    }

    class RequestSenderMock : IRequestSender
    {
        private ToDoItem[] _itemsStorage = new ToDoItem[10].Select(x => new ToDoItem()).ToArray();

        public ActionErrorMock[] ActionErrors;

        public async Task<Result<IEnumerable<ToDoItem>, RequestError>> GetTasksAsync()
        {
            var reqError = ActionErrors.FirstOrDefault(e => e.Action == ApiAction.List);
            if (reqError == null)
                return Result.Ok<IEnumerable<ToDoItem>, RequestError>(_itemsStorage);
            return Result.Fail<IEnumerable<ToDoItem>, RequestError>(reqError.Error);
        }

        public async Task<Result<object, RequestError>> SendRequestAsync(ToDoItem item, ApiAction action)
        {
            var sendError = ActionErrors?.FirstOrDefault(e => e.Action == action);
            if (sendError == null)
                return Result.Ok<object, RequestError>(new { });
            return Result.Fail<object, RequestError>(sendError.Error);
        }
    }
}
