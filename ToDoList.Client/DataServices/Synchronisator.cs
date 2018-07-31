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
    public class Synchronisator
    {
        private Uri http;

        public bool IsSyncSuccessful;

        private readonly TimeSpan WaitingTime = TimeSpan.FromSeconds(40);

        private Synchronisator()
        {
            http = new Uri("http://localhost:51650/");
        }

        private static bool _isInitialised;

        public static Synchronisator SyncInit()
        {
            if (!_isInitialised)
            {
                _isInitialised = true;
                return new Synchronisator();
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

        public async Task<Result<HttpResponseMessage>> SendRequestAsync<T>(T body, ApiAction action)
            => await RequestExceptionsHandle(async () =>
            {
                using (var client = new HttpClient { Timeout = WaitingTime })
                {
                    var message = action == ApiAction.Delete
                    ? new HttpRequestMessage(action.Method, new Uri(http, $"{action.Name}/{body}"))
                    : ConfigureMessage(body, action);

                    var res = await client.SendAsync(message);

                    if (res.StatusCode == HttpStatusCode.NoContent)
                        return Result.Ok(res);

                    return Result.Fail<HttpResponseMessage>(GetResponseRepresentation(res));
                }
            });

        public async Task<Result<IEnumerable<ToDoItem>>> GetTasksAsync()
            => await RequestExceptionsHandle(async () =>
              {
                  using (var client = new HttpClient { Timeout = WaitingTime })
                  {
                      var res = await client.GetAsync(new Uri(http, ApiAction.List.Name));
                      var json = await res.Content.ReadAsStringAsync();

                      if (res.StatusCode == HttpStatusCode.OK)
                      {
                          var resValue = JsonConvert.DeserializeObject<IEnumerable<ToDoItem>>(json);
                          return Result.Ok(resValue);
                      }

                      return Result.Fail<IEnumerable<ToDoItem>>(GetResponseRepresentation(res));
                  }
              });

        public async Task<Result<T>> RequestExceptionsHandle<T>(Func<Task<Result<T>>> handledScope)
        {
            try
            {
                return await handledScope();
            }
            catch (HttpRequestException)
            {
                return Result.Fail<T>("Server is unreachable. Try another port or something.");
            }
            catch (TaskCanceledException)
            {
                return Result.Fail<T>("Too long waiting for the response.");
            }
        }

        private string GetResponseRepresentation(HttpResponseMessage message)
            => $"Error {(int)message.StatusCode}: {message.ReasonPhrase}";
    }
}
