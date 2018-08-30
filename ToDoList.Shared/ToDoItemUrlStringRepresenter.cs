using System;
using System.Globalization;
using System.Linq;

namespace ToDoList.Shared
{
    public class ToDoItemUrlStringRepresenter
    {
        public const string DATE_FORMAT = "yy-MM-dd HH_mm_ss_fff";

        public int IndexInList { get; private set; }

        public DateTime Timestamp { get; private set; }

        private ToDoItem _representedItem;

        private ToDoItemUrlStringRepresenter() { }

        public ToDoItemUrlStringRepresenter(ToDoItem item)
        {
            _representedItem = item;
            IndexInList = _representedItem.List.ToDoItems.ToList().IndexOf(_representedItem);
            Timestamp = _representedItem.Timestamp;
        }

        public override string ToString()
            => $"[{IndexInList}]{Timestamp.ToString(DATE_FORMAT)}";

        public static ToDoItemUrlStringRepresenter Parse(string source)
        {
            var parts = source.Split('[', ']');
            return new ToDoItemUrlStringRepresenter
            {
                IndexInList = int.Parse(parts[1]),
                Timestamp = DateTime.ParseExact(parts[2], DATE_FORMAT, CultureInfo.InvariantCulture),
            };
        }

    }
}
