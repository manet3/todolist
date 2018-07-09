using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;

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

        public static bool IsSynchronized;

        static Synchronisator()
        {
            Client = new HttpClient();
            http = new Uri("http://localhost:51129/api/tasks/");
        }

        public static async Task AddAsync(IEnumerable<TaskModel> itemsToAdd) 
            => await SendRequestAsync(itemsToAdd, HttpMethod.Put);

        public static async Task AddAsync(TaskModel item) 
            => await SendRequestAsync(item, HttpMethod.Post);

        private static async Task SendRequestAsync<T>(T task, HttpMethod requestType)
        {
            var endpoint = requestType == HttpMethod.Put ? "change" : "add";

            var json = JsonManager.MakeJson(task);
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
                        return JsonManager.FromJson<HashSet<TaskModel>>(await jsonTask);
                    else
                        return new HashSet<TaskModel>();
                }
            }
        }

        public static async Task DeleteItemAsync(TaskModel task)
        {
            using (Task<HttpResponseMessage> res_task = Client.DeleteAsync(new Uri(http, $"remove/{task.Id}")))
            {
                var result = await res_task;
                SynchChanged?.Invoke(ClassifyResponse(result));
            }
        }

        /// <summary>
        /// Called from sync code if task is executed at once.
        /// </summary>
        public static void LoadingStartedInvoke()
        {
            SynchChanged?.Invoke(SyncState.Started);
        }

        private static SyncState ClassifyResponse(HttpResponseMessage message)
        {
            var res_type = message.StatusCode == HttpStatusCode.OK
                || message.StatusCode == HttpStatusCode.NoContent
                ? SyncState.Complete
                : SyncState.Failed;
            IsSynchronized = res_type != SyncState.Failed;
            return res_type;
        }
    }
}
