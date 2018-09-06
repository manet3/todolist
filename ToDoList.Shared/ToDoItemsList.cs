using System.Collections.Generic;

namespace ToDoList.Shared
{
    public class ToDoItemsList : SyncEntityBase
    {
        public IEnumerable<ToDoItem> Items { get; set; }
    }
}
