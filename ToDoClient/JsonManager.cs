using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ToDoList.Client
{
    public static class JsonManager
    {
        public static string Json { get; private set; }

        public static string MakeJson(object serialized)
        {
            Json = JsonConvert.SerializeObject(serialized);
            return Json;
        }

        public static T FromJson<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
