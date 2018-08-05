using System;
using System.Globalization;

namespace ToDoList.Shared
{
    public class ToDoItem
    {
        public DateTime Timestamp { get; set; }

        public string Name { get; set; }

        public bool IsChecked { get; set; }

        public const string DATE_FORMAT = "yy-mm-dd hh_mm_ss_fff";

        public override string ToString()
            => $"{Name}[{Timestamp.ToString(DATE_FORMAT)}]";

        public static ToDoItem Parse(string repr)
        {
            var parts = repr.Split('[',']');
            return new ToDoItem { Name = parts[0],
                Timestamp = DateTime.ParseExact(parts[1], DATE_FORMAT, CultureInfo.InvariantCulture) };
        }

        public override bool Equals(object obj)
            => obj is ToDoItem && Name.Equals(((ToDoItem)obj).Name);

        public override int GetHashCode()
            => Name.GetHashCode();

    }
}
