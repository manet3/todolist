namespace ToDoList.Client
{
    public class TaskModel
    {
        public bool IsChecked { get; set; }

        public string Name { get; set; }

        public override bool Equals(object obj)
            => obj is TaskModel && ((TaskModel)obj).Name.Equals(Name);

        public override int GetHashCode()
            => Name.GetHashCode();
    }
}
