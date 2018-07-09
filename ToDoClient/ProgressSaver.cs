using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ToDoList.Client
{
    public static class ProgressSaver<T>
    {
        private const string DataPath = "Sessions";

        public static IEnumerable<T> TryGetSessions()
        {
            var res = new List<T>();

            if (!Directory.Exists(DataPath))
                return res;

            string[] ses_paths = Directory.GetFiles(DataPath);


            foreach (var p in ses_paths)
            {
                var text = File.ReadAllText(p);
                res.Add(JsonManager.FromJson<T>(text));
                File.Delete(p);
            }
            return res;
        }

        public static void SaveSessionJson(string json)
        {
            Directory.CreateDirectory(DataPath);
            File.WriteAllText($@"{DataPath}\session.json", json);
        }

        public static void SaveLastSession()
            => SaveSessionJson(JsonManager.Json);

        public static void SaveCurrentSession(T session)
            => SaveSessionJson(JsonManager.MakeJson(session));
    }
}
