using ServiceStack.DataAnnotations;
using System;

namespace ToDoList.Server.Database.POCOs
{
    public class ItemPoco : PocoCommon
    {
        public ulong ListPocoId { get; set; }

        public bool IsChecked { get; set; }
    }
}