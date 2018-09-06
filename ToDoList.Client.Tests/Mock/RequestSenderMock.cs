using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ToDoList.Client.DataServices;
using ToDoList.Shared;

namespace ToDoList.Client.Tests.Mock
{
    public class ErrorOnAction
    {
        public ApiAction Action;

        public RequestError Error;

        public ErrorOnAction(ApiAction action, RequestErrorType errorType)
            => (Action, Error) = (action, new RequestError(string.Empty, errorType));

        public ErrorOnAction(ApiAction action, RequestError error)
            => (Action, Error) = (action, error);
    }

    class RequestSenderMock : IRequestSender
    {
        public ErrorOnAction[] ActionErrors;

        public async Task<RequestResult<IEnumerable<ToDoItemsList>>> GetTasksAsync()
        {
            var reqError = ActionErrors?.FirstOrDefault(e => e.Action == ApiAction.List);
            if (reqError == null)
                return RequestResult.Ok<IEnumerable<ToDoItemsList>>(new ToDoItemsList[10]);

            return RequestResult.Fail<IEnumerable<ToDoItemsList>>(reqError.Error);
        }

        public async Task<RequestResult> SendRequestAsync(ToDoItem item, ApiAction action)
        {
            var sendError = ActionErrors?.FirstOrDefault(e => e.Action == action);
            if (sendError == null)
                return RequestResult.Ok();

            return RequestResult.Fail(sendError.Error);
        }
    }
}
