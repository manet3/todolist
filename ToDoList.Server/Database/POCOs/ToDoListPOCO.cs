using ServiceStack.DataAnnotations;

namespace ToDoList.Server.Database.POCOs
{
    public class ToDoListPoco
    {
        [AutoIncrement]
        public ulong Id { get; set; }

        [Unique]
        public string Name { get; set; }

        [Reference]
        public ToDoItemPoco[] ToDoItems { get; set; }
    }
}