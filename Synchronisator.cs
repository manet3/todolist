using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using System.IO;
using System.Net.Http.Headers;

namespace ToDoList.Client
{
    public enum SyncState
    {
        Started,
        Complete,
        Failed
    }

    static class Synchronisator
    {
        private static readonly HttpClient Client;

        public static event Action<SyncState> SynchChanged;

        private static Uri http;

        static Synchronisator()
        {
            Client = new HttpClient();
            http = new Uri("http://localhost:51129/api/tasks/");
        }

        public static async Task Add(IEnumerable<TaskModel> itemsToAdd) 
            => await SendRequest(itemsToAdd, HttpMethod.Put);

        public static async Task Add(TaskModel item) 
            => await SendRequest(item, HttpMethod.Post);

        private static async Task SendRequest<T>(T task, HttpMethod requestType)
        {
            var endpoint = requestType == HttpMethod.Put ? "change" : "add";

            SynchChanged?.Invoke(SyncState.Started);

            var json = JsonConvert.SerializeObject(task);
            var req_message = new HttpRequestMessage(requestType, new Uri(http, endpoint))
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            using (Task<HttpResponseMessage> putTask = Client.SendAsync(req_message))
            {
                SynchChanged?.Invoke(ClassifyResponse(await putTask));
            }
        }

        public static async Task<HashSet<TaskModel>> GetTasksAsync()
        {
            SynchChanged?.Invoke(SyncState.Started);
            using (Task<HttpResponseMessage> resp_message = Client.GetAsync(new Uri(http, "list")))
            {
                var res = await resp_message;

                using (Task<string> jsonTask = res
                        .Content
                        .ReadAsStringAsync())
                {
                    var res_type = ClassifyResponse(res);
                    SynchChanged?.Invoke(res_type);

                    if (res_type == SyncState.Complete)
                        return JsonConvert.DeserializeObject<HashSet<TaskModel>>(await jsonTask);
                    else
                        return new HashSet<TaskModel>();
                }
            }
        }

        public static async Task DeleteItemAsync(TaskModel task)
        {
            SynchChanged?.Invoke(SyncState.Started);
            using (Task<HttpResponseMessage> res_task = Client.DeleteAsync(new Uri(http, $"remove/{task.Id}")))
            {
                var result = await res_task;
                SynchChanged?.Invoke(ClassifyResponse(result));
            }
        }

        private static SyncState ClassifyResponse(HttpResponseMessage message)
            => message.StatusCode == HttpStatusCode.OK 
            || message.StatusCode == HttpStatusCode.NoContent
            ? SyncState.Complete
            : SyncState.Failed;
    }
}
