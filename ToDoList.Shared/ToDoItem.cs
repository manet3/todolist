using System;
using System.Globalization;

namespace ToDoList.Shared
{
    public class ToDoItem
    {
        public const string DATE_FORMAT = "O";

        private string _name;
        public string Name
        {
            get => _name;
            set => RefreshTimestampFor(ref _name, value);
        }

        private bool _isChecked;
        public bool IsChecked
        {
            get => _isChecked;
            set => RefreshTimestampFor(ref _isChecked, value);
        }

        public DateTime Timestamp { get; private set; }

        private void RefreshTimestampFor<T>(ref T field, T value)
        {
            field = value;
            Timestamp = DateTime.UtcNow;
        }

        public override string ToString()
            => $"{Name}|{Timestamp.ToString(DATE_FORMAT)}";

        public static ToDoItem Parse(string repr)
        {
            var parts = repr.Split('|');
            return new ToDoItem { Name = parts[0],
                Timestamp = DateTime.ParseExact(parts[1], DATE_FORMAT, CultureInfo.InvariantCulture) };
        }

        public override bool Equals(object obj)
            => obj is ToDoItem && Name.Equals(((ToDoItem)obj).Name);

        public override int GetHashCode()
            => Name.GetHashCode();

    }
}
