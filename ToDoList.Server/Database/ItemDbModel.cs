using ServiceStack.DataAnnotations;
using System;

namespace ToDoList.Server.Database.Models
{
    public class ItemDbModel
    {
        [AutoIncrement]
        public ulong Id { get; set; }

        [Required, Unique]
        public string Name { get; set; }

        public bool IsChecked { get; set; }

        [Required]
        public DateTime TimeStamp { get;  set;}

    }
}