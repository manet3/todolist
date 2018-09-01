using ServiceStack.DataAnnotations;
using System;

namespace ToDoList.Server.Database.POCOs
{
    public class PocoCommon
    {
        [AutoIncrement]
        public ulong Id { get; set; }

        [Required, Unique]
        public string Name { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }
    }
}