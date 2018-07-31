using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using ToDoList.Shared;

namespace ToDoList.Client.DataServices
{
    public class LocalStorage
    {
        private const string SESSIONS_PATH = "Sessions";

        private static bool _isInited;

        private LocalStorage() { }

        public static LocalStorage InitStorage()
        {
            if (_isInited)
                return null;

            _isInited = true;
            return new LocalStorage();
        }

        public IEnumerable<ToDoItem> GetSessions()
        {
            var res = new List<ToDoItem>();

            if (!Directory.Exists(SESSIONS_PATH))
                return res;

            foreach (var path in Directory.GetFiles(SESSIONS_PATH))
            {
                var json = File.ReadAllText(path);
                res.AddRange(JsonConvert.DeserializeObject<IEnumerable<ToDoItem>>(json));
                File.Delete(path);
            }
            return res;
        }

        public void SaveSession(IEnumerable<ToDoItem> items)
        {
            Directory.CreateDirectory(SESSIONS_PATH);
            File.WriteAllText($@"{SESSIONS_PATH}\session.json", JsonConvert.SerializeObject(items));
        }
    }
}
