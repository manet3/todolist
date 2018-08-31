namespace ToDoList.Shared
{
    public class ToDoItem : SyncEntityBase
    {
        public bool IsChecked { get; set; }

        public ToDoItemsList List { get; set; }
    }
}
