﻿using System.Collections.Generic;

namespace ToDoList.Shared
{
    public class ToDoItemsList : SyncEntityBase
    {
        public HashSet<ToDoItem> ToDoItems { get; set; }
    }
}