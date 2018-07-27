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
    public enum SyncState
    {
        Started,
        Complete,
        Failed,
        Impossible
    }

    public class Synchronisator
    {
        public event Action<SyncState> SynchChanged;

        private Uri http;

        public bool IsSyncSuccessful;

        private readonly TimeSpan WaitingTime = TimeSpan.FromSeconds(20);

        private Synchronisator()
        {
            http = new Uri("http://localhost:51129/api/tasks/");
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

        private Dictionary<HttpMethod, string> _methodNames
            = new Dictionary<HttpMethod, string>
            {
                { HttpMethod.Put,  "change"},
                { HttpMethod.Post,  "add"},
                { HttpMethod.Get,  "list" }
            };

        public async Task<Result> PatchAsync(IEnumerable<ToDoItem> items)
            => await SendRequestAsync(ConfigureMessage(items, HttpMethod.Put));

        public async Task<Result> PatchAsync(ToDoItem item)
            => await SendRequestAsync(ConfigureMessage(item, HttpMethod.Put));

        public async Task<Result> AddAsync(ToDoItem item)
            => await SendRequestAsync(ConfigureMessage(item, HttpMethod.Post));

        private HttpRequestMessage ConfigureMessage<T>(T body, HttpMethod method)
            => new HttpRequestMessage(method, new Uri(http, _methodNames[method]))
            {
                Content = new StringContent(
                      JsonConvert.SerializeObject(body)
                      , Encoding.UTF8
                      , "application/json")
            };

        private async Task<Result> SendRequestAsync(HttpRequestMessage message)
            => await RequestExceptionsHandle(async () =>
            {
                using (var client = new HttpClient { Timeout = WaitingTime })
                {
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
                      var res = await client.GetAsync(new Uri(http, _methodNames[HttpMethod.Get]));
                      var json = await res.Content.ReadAsStringAsync();

                      if (res.StatusCode == HttpStatusCode.OK)
                      {
                          var resValue = JsonConvert.DeserializeObject<IEnumerable<ToDoItem>>(json);
                          return Result.Ok(resValue);
                      }

                      return Result.Fail<IEnumerable<ToDoItem>>(GetResponseRepresentation(res));
                  }
              });

        public async Task<Result> DeleteItemAsync(ToDoItem task)
            => await RequestExceptionsHandle( async () =>
            {
                using (var client = new HttpClient { Timeout = WaitingTime })
                {
                    var result = await client.DeleteAsync(new Uri(http, $"remove/{task.Name}"));

                    if (result.StatusCode == HttpStatusCode.NoContent)
                        return Result.Ok(result);

                    return Result.Fail<HttpResponseMessage>(GetResponseRepresentation(result));
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
            => $"Error {(int)message.StatusCode}: {message.ReasonPhrase}.";
    }
}
