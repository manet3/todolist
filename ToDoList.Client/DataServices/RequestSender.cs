using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using ToDoList.Shared;
using Newtonsoft.Json;

namespace ToDoList.Client.DataServices
{
    public interface IRequestSender
    {
        Task<RequestResult> SendRequestAsync(ToDoItem item, ApiAction action);

        Task<RequestResult<IEnumerable<ToDoItem>>> GetTasksAsync();
    }

    public class RequestSender : IRequestSender
    {
        const int MAX_WAITING_TIME_SEC = 30;

        private Uri _http;

        public bool IsSyncSuccessful;

        public RequestSender()
            => _http = new Uri("http://localhost:51650/");

        private HttpRequestMessage ConfigureMessage(ToDoItem item, ApiAction action)
            => action.Method == HttpMethod.Delete
            ? new HttpRequestMessage(HttpMethod.Delete, new Uri(_http, $"{action.Name}/{new ToDoItemUrlStringRepresenter(item)}"))
            : new HttpRequestMessage(action.Method, new Uri(_http, action.Name))
            {
                Content = new StringContent(
                        JsonConvert.SerializeObject(item),
                        Encoding.UTF8,
                        "application/json")
            };

        public async Task<RequestResult> SendRequestAsync(ToDoItem item, ApiAction action)
            => await RequestExceptionsHandle(async () =>
            {
                using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(MAX_WAITING_TIME_SEC) })
                {
                    var message = ConfigureMessage(item, action);

                    var res = await client.SendAsync(message);

                    if (res.StatusCode == HttpStatusCode.NoContent)
                        return RequestResult.Ok(res);

                    return RequestResult.Fail<HttpResponseMessage>(GetResponseErrorRepresentation(res));
                }
            });

        public async Task<RequestResult<IEnumerable<ToDoItem>>> GetTasksAsync()
            => await RequestExceptionsHandle(async () =>
              {
                  using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(MAX_WAITING_TIME_SEC) })
                  {
                      var res = await client.GetAsync(new Uri(_http, ApiAction.List.Name));
                      var json = await res.Content.ReadAsStringAsync();

                      if (res.StatusCode == HttpStatusCode.OK)
                      {
                          var resValue = JsonConvert.DeserializeObject<IEnumerable<ToDoItem>>(json);
                          return RequestResult.Ok(resValue);
                      }

                      return RequestResult.Fail<IEnumerable<ToDoItem>>(GetResponseErrorRepresentation(res));
                  }
              });

        public async Task<RequestResult<T>> RequestExceptionsHandle<T>(Func<Task<RequestResult<T>>> handledScope)
        {
            try
            {
                return await handledScope();
            }
            catch (HttpRequestException)
            {
                return RequestResult.Fail<T>(
                    new RequestError("Server is unreachable. Try another port or something.", RequestErrorType.NoConnection));
            }
            catch (TaskCanceledException)
            {
                return RequestResult.Fail<T>(
                    new RequestError("Too long waiting for the response.", RequestErrorType.Cancelled));
            }
        }

        private RequestError GetResponseErrorRepresentation(HttpResponseMessage message)
            => new RequestError($"Error {(int)message.StatusCode}: {message.ReasonPhrase}", RequestErrorType.ServerError);
    }
}
