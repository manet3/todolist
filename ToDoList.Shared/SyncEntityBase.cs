using System;

namespace ToDoList.Shared
{
    public class SyncEntityBase : IEquatable<SyncEntityBase>
    {
        public ulong Id { get; set; }

        public string Name { get; set; }

        public DateTime Timestamp { get; private set; }

        public void UpdateTimestamp()
            => Timestamp = DateTime.UtcNow;

        public bool Equals(SyncEntityBase other)
            => Name == other.Name;

        public override int GetHashCode()
            => Name.GetHashCode();
    }

}
