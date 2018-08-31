using ServiceStack.DataAnnotations;
using System;

namespace ToDoList.Server.Database.POCOs
{
    public class ToDoItemPoco
    {
        [AutoIncrement]
        public ulong Id { get; set; }

        public ulong ToDoListPocoId { get; set; }

        [Required, Unique]
        public string Name { get; set; }

        public bool IsChecked { get; set; }

        [Required]
        public DateTime Timestamp { get;  set;}
    }
}