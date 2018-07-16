namespace ToDoList.Shared
{
    public class ToDoItem
    {
        public ulong Id { get; set; }
        public string Name { get; set; }
        public bool IsChecked { get; set; }
        
        public override bool Equals(object obj)
            => obj is ToDoItem && Name.Equals(((ToDoItem)obj).Name);

        public override int GetHashCode()
            => Name.GetHashCode();

    }
}
