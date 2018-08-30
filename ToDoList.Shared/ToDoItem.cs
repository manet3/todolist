using System;

namespace ToDoList.Shared
{
    public class ToDoItem : IEquatable<ToDoItem>
    {
        public string Name { get; set; }

        public bool IsChecked { get; set; }

        public ToDoList List { get; set; }

        public DateTime Timestamp { get; private set; }

        public void UpdateTimestamp()
            => Timestamp = DateTime.UtcNow;

        public override int GetHashCode()
            => Name.GetHashCode();

        public bool Equals(ToDoItem other)
            => Name == other.Name && List == other.List;
    }
}
