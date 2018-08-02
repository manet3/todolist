using System;

namespace ToDoList.Shared
{
    public class ToDoItem
    {
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

        public override bool Equals(object obj)
            => obj is ToDoItem && Name.Equals(((ToDoItem)obj).Name);

        public override int GetHashCode()
            => Name.GetHashCode();

    }
}
