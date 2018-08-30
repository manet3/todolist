using System;
using System.Collections.Generic;

namespace ToDoList.Shared
{
    public class ToDoList : IEquatable<ToDoList>
    {
        public ulong Id { get; set; }

        public string Name { get; set; }

        public HashSet<ToDoItem> ToDoItems { get; set; }

        public bool Equals(ToDoList other)
            => Name == other.Name;

        public override int GetHashCode()
            => Name.GetHashCode();
    }
}
