namespace ToDoList.Client
{
    public class ToDoItemModel
    {
        public bool IsChecked { get; set; }

        public string Name { get; set; }

        public override bool Equals(object obj)
            => obj is ToDoItemModel && ((ToDoItemModel)obj).Name.Equals(Name);

        public override int GetHashCode()
            => Name.GetHashCode();
    }
}
