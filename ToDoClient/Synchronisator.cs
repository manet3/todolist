using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Threading;

namespace ToDoList.Client
{
    public enum SyncState
    {
        Started,
        Complete,
        Failed,
        Impossible
    }

    static class Synchronisator
    {
        public static event Action<SyncState> SynchChanged;

        private static Uri http;

        public static bool IsSyncSuccessful;

        private static readonly TimeSpan WaitingTime = TimeSpan.FromSeconds(20);

        static Synchronisator()
        {
            http = new Uri("http://localhost:51129/api/tasks/");
        }

        public static async Task AddAsync(IEnumerable<TaskModel> itemsToAdd) 
            => await SendRequestAsync(itemsToAdd, HttpMethod.Put);

        public static async Task AddAsync(TaskModel item) 
            => await SendRequestAsync(item, HttpMethod.Post);

        private static async Task SendRequestAsync<T>(T task, HttpMethod requestType)
        {
            //configuring message
            var endpoint = requestType == HttpMethod.Put ? "change" : "add";
            var json = JsonManager.MakeJson(task);
            var req_message = new HttpRequestMessage(requestType, new Uri(http, endpoint))
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            //sending request
            try
            {
                using (var client = new HttpClient { Timeout = WaitingTime })
                    SynchChanged?.Invoke(ClassifyResponse(await client.SendAsync(req_message)));
            }
            catch (Exception ex)
            {
                IsSyncSuccessful = false;
                if (ex is HttpRequestException)
                    SynchChanged?.Invoke(SyncState.Impossible);
                if (ex is TaskCanceledException)
                    SynchChanged?.Invoke(SyncState.Failed);
            }
        }

        public static async Task<HashSet<TaskModel>> GetTasksAsync()
        {
            try
            {
                using (var client = new HttpClient { Timeout = WaitingTime })
                {
                    var res = await client.GetAsync(new Uri(http, "list"));
                    var json = await res.Content.ReadAsStringAsync();

                    var res_type = ClassifyResponse(res);

                    SynchChanged?.Invoke(res_type);

                    if (res_type == SyncState.Complete)
                        return JsonManager.FromJson<HashSet<TaskModel>>(json);
                    else
                        return new HashSet<TaskModel>();
                }
            }
            catch (Exception ex)
            {
                IsSyncSuccessful = false;
                if (ex is HttpRequestException)
                    SynchChanged?.Invoke(SyncState.Impossible);
                if(ex is TaskCanceledException)
                    SynchChanged?.Invoke(SyncState.Failed);
                return new HashSet<TaskModel>();
            }

        }

        public static async Task DeleteItemAsync(TaskModel task)
        {
            try
            {
                using (var client = new HttpClient { Timeout = WaitingTime })
                {
                    var result = await client.DeleteAsync(new Uri(http, $"remove/{task.Id}"));
                    SynchChanged?.Invoke(ClassifyResponse(result));
                }
            }
            catch (Exception ex)
            {
                IsSyncSuccessful = false;
                if (ex is HttpRequestException)
                    SynchChanged?.Invoke(SyncState.Impossible);
                if (ex is TaskCanceledException)
                    SynchChanged?.Invoke(SyncState.Failed);
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
            IsSyncSuccessful = res_type != SyncState.Failed;
            return res_type;
        }

    }
}
