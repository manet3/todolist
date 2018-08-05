using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using ToDoList.Shared;
using Newtonsoft.Json;
using CSharpFunctionalExtensions;

namespace ToDoList.Client.DataServices
{
    public enum RequestErrorType
    {
        None,
        Cancelled,
        NoConnection,
        ServerError
    }

    public class RequestError
    {
        public string Message;

        public RequestErrorType ErrorType;

        public RequestError(string message,
            RequestErrorType errorType = RequestErrorType.None)
            => (Message, ErrorType) = (message, errorType);

    }

    public class RequestSender
    {
        private Uri http;

        public bool IsSyncSuccessful;

        private readonly TimeSpan WaitingTime = TimeSpan.FromSeconds(40);

        private RequestSender()
        {
            http = new Uri("http://localhost:51650/");
        }

        private static bool _isInitialised;

        public static RequestSender SyncInit()
        {
            if (!_isInitialised)
            {
                _isInitialised = true;
                return new RequestSender();
            }
            else return null;
        }

        private HttpRequestMessage ConfigureMessage<T>(T body, ApiAction method)
            => new HttpRequestMessage(method.Method, new Uri(http, method.Name))
            {
                Content = new StringContent(
                      JsonConvert.SerializeObject(body)
                      , Encoding.UTF8
                      , "application/json")
            };

        public async Task<Result<object, RequestError>> SendRequestAsync(ToDoItem item, ApiAction action)
            => await RequestExceptionsHandle(async () =>
            {
                using (var client = new HttpClient { Timeout = WaitingTime })
                {
                    var message = action.Equals(ApiAction.Delete)
                    ? new HttpRequestMessage(action.Method, new Uri(http, $"{action.Name}/{item.ToString()}"))
                    : ConfigureMessage(item, action);

                    var res = await client.SendAsync(message);

                    if (res.StatusCode == HttpStatusCode.NoContent)
                        return Result.Ok<object, RequestError>(res);

                    return Result.Fail<object, RequestError>(GetResponseErrorRepresentation(res));
                }
            });

        public async Task<Result<IEnumerable<ToDoItem>, RequestError>> GetTasksAsync()
            => await RequestExceptionsHandle(async () =>
              {
                  using (var client = new HttpClient { Timeout = WaitingTime })
                  {
                      var res = await client.GetAsync(new Uri(http, ApiAction.List.Name));
                      var json = await res.Content.ReadAsStringAsync();

                      if (res.StatusCode == HttpStatusCode.OK)
                      {
                          var resValue = JsonConvert.DeserializeObject<IEnumerable<ToDoItem>>(json);
                          return Result.Ok<IEnumerable<ToDoItem>, RequestError>(resValue);
                      }

                      return Result.Fail<IEnumerable<ToDoItem>, RequestError>(GetResponseErrorRepresentation(res));
                  }
              });

        public async Task<Result<T, RequestError>> RequestExceptionsHandle<T>(Func<Task<Result<T, RequestError>>> handledScope)
        {
            try
            {
                return await handledScope();
            }
            catch (HttpRequestException)
            {
                return Result.Fail<T, RequestError>(
                    new RequestError("Server is unreachable. Try another port or something.", RequestErrorType.NoConnection));
            }
            catch (TaskCanceledException)
            {
                return Result.Fail<T, RequestError>(
                    new RequestError("Too long waiting for the response.", RequestErrorType.Cancelled));
            }
        }

        private RequestError GetResponseErrorRepresentation(HttpResponseMessage message)
            => new RequestError($"Error {(int)message.StatusCode}: {message.ReasonPhrase}", RequestErrorType.ServerError);
    }
}
