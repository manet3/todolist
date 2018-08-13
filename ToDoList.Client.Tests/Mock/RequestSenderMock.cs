using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ToDoList.Client.DataServices;
using ToDoList.Shared;

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
        private IEnumerable<ToDoItem> _itemsStorage = new ToDoItem[10].Select(x => new ToDoItem());

        public ActionErrorMock[] ActionErrors;

        public async Task<RequestResult<IEnumerable<ToDoItem>>> GetTasksAsync()
        {
            var reqError = ActionErrors.FirstOrDefault(e => e.Action == ApiAction.List);
            if (reqError == null)
                return RequestResult.Ok(_itemsStorage);

            return RequestResult.Fail<IEnumerable<ToDoItem>>(reqError.Error);
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
