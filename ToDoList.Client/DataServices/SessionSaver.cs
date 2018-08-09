using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using ToDoList.Shared;

namespace ToDoList.Client.DataServices
{
    public class SessionSaver
    {
        private const string SESSION_PATH = "session.json";

        private const char SEPARATOR = '♥';

        private Type _mementoType;

        public object SyncMemento { get; private set; }

        public IEnumerable<ToDoItem> List { get; private set; }

        private SessionSaver() { }

        public SessionSaver(object syncMemento, Type mementoType, IEnumerable<ToDoItem> list)
            => (SyncMemento, _mementoType, List) = (syncMemento, mementoType, list);

        public void SaveJson()
        {
            var json = string.Join(SEPARATOR.ToString(), 
                JsonConvert.SerializeObject(List),
                JsonConvert.SerializeObject(SyncMemento, _mementoType, null));

            File.WriteAllText(SESSION_PATH, json);
        }

        public static SessionSaver FromJson(Type mementoType)
        {
            if (!File.Exists(SESSION_PATH))
                return null;

            var jsonParts = File.ReadAllText(SESSION_PATH).Split(SEPARATOR);

            File.Delete(SESSION_PATH);
            try
            {
                return new SessionSaver
                {
                    List = JsonConvert.DeserializeObject<IEnumerable<ToDoItem>>(jsonParts[0]),
                    SyncMemento = JsonConvert.DeserializeObject(jsonParts[1], mementoType)
                };
            }
            //file corruption
            catch (JsonReaderException)
            {
                return null;
            }
        }
    }
}
