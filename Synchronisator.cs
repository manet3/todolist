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
    public enum SynchState
    {
        Started,
        Complete,
        Failed
    }

    static class Synchronisator
    {
        private static readonly HttpClient Client;

        public static event Action<SynchState> SynchChanged;

        private static Uri http;

        static Synchronisator()
        {
            Client = new HttpClient();
            http = new Uri("http://localhost:51129/api/tasks/");
        }

        public static void Add(IEnumerable<TaskModel> itemsToAdd) => SendRequest(itemsToAdd, HttpMethod.Put);

        public static void Add(TaskModel item) => SendRequest(item, HttpMethod.Post);

        private static void SendRequest<T>(T task, HttpMethod requestType)
        {
            var endpoint = requestType == HttpMethod.Put ? "change" : "add";
            SynchChanged?.Invoke(SynchState.Started);

            var json = JsonConvert.SerializeObject(task);
            var req_message = new HttpRequestMessage(requestType, new Uri(http, endpoint))
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            using (Task<HttpResponseMessage> putTask = Client.SendAsync(req_message))
            {
                putTask.Wait();
                var state = putTask.Result.StatusCode == HttpStatusCode.OK
                    ? SynchState.Complete
                    : SynchState.Failed;
                SynchChanged?.Invoke(state);
            }
        }

        public static HashSet<TaskModel> GetTasks()
        {
            SynchChanged?.Invoke(SynchState.Started);
            using (Task<HttpResponseMessage> resp_message = Client.GetAsync(new Uri(http, "list")))
            using (Task<string> jsonTask = resp_message.Result
                    .Content
                    .ReadAsStringAsync())
            {
                jsonTask.Wait();
                var json = jsonTask.Result;

                SynchChanged?.Invoke(SynchState.Complete);

                return JsonConvert.DeserializeObject<HashSet<TaskModel>>(json);
            }
        }

        public static void DeleteTask(TaskModel task)
        {
            SynchChanged?.Invoke(SynchState.Started);
            Task <HttpResponseMessage> res = Client.DeleteAsync(new Uri(http, $"remove/{task.Id}"));

            res.Wait();
            if(res.Result.StatusCode == HttpStatusCode.OK)
                SynchChanged?.Invoke(SynchState.Complete);

        }
    }
}
