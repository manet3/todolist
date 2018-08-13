﻿using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using ToDoList.Shared;

namespace ToDoList.Client.DataServices
{
    public class SessionSaver
    {
        private const string SESSION_PATH = "session.json";

        public object SyncState { get; private set; }

        public IEnumerable<ToDoItem> List { get; private set; }

        private SessionSaver() { }

        public SessionSaver(object syncState, IEnumerable<ToDoItem> list)
            => (SyncState, List) = (syncState, list);

        public void SaveJson()
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented,
                new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });

            File.WriteAllText(SESSION_PATH, json);
        }

        public static SessionSaver FromJson()
        {
            if (!File.Exists(SESSION_PATH))
                return null;

            var json = File.ReadAllText(SESSION_PATH);

            File.Delete(SESSION_PATH);

            try
            {
                return JsonConvert.DeserializeObject<SessionSaver>(json,
                    new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
            }
            //file corruption
            catch (JsonException)
            {
                return null;
            }
        }
    }
}
