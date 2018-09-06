using System;

namespace ToDoList.Shared
{
    public class SyncEntityBase
    {
        public ulong Id { get; set; }

        public string Name { get; set; }

        public DateTime Timestamp { get; protected set; }

        public void UpdateTimestamp()
            => Timestamp = DateTime.UtcNow;

        public override bool Equals(object obj)
            => obj is SyncEntityBase otherEntity && otherEntity.Name == Name;

        public override int GetHashCode()
            => Name.GetHashCode();
    }

}
