using System;
using System.Globalization;
namespace ToDoList.Shared
{
    public class SyncEntityUrlStringRepresentation
    {
        public const string DATE_FORMAT = "yy-MM-dd HH_mm_ss_fff";

        public ulong Id { get; private set; }

        public DateTime Timestamp { get; private set; }

        private SyncEntityBase _entity;

        private SyncEntityUrlStringRepresentation() { }

        public SyncEntityUrlStringRepresentation(SyncEntityBase entityRepresented)
        {
            _entity = entityRepresented;
            Timestamp = _entity.Timestamp;
            Id = _entity.Id;
        }

        public override string ToString()
            => $"[{Id}]{Timestamp.ToString(DATE_FORMAT)}";

        public static SyncEntityUrlStringRepresentation Parse(string source)
        {
            var parts = source.Split('[', ']');
            return new SyncEntityUrlStringRepresentation
            {
                Id = ulong.Parse(parts[1]),
                Timestamp = DateTime.ParseExact(parts[2], DATE_FORMAT, CultureInfo.InvariantCulture),
            };
        }

    }
}
