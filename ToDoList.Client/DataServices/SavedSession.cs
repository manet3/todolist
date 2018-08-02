using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using ToDoList.Shared;

namespace ToDoList.Client.DataServices
{
    public class SavedSession
    {
        private const string SESSION_PATH = "session.json";

        public SyncMemento SyncState { get; }

        public IEnumerable<ToDoItem> List { get; }

        public SavedSession(SyncMemento syncState,
            IEnumerable<ToDoItem> list)
            => (SyncState, List) = (syncState, list);

        public void SaveJson() 
            => File.WriteAllText(SESSION_PATH, JsonConvert.SerializeObject(this));

        public static SavedSession FromJson()
        {
            if (!File.Exists(SESSION_PATH))
                return null;

            var json = File.ReadAllText(SESSION_PATH);
            //File.Delete(SESSION_PATH);

            try
            {
                return JsonConvert.DeserializeObject<SavedSession>(json);
            }
            //file corruption
            catch(JsonReaderException)
            {
                return null;
            }
        }
    }
}
