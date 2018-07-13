namespace ToDoList.Client
{
    public class TaskModel
    {
        public bool State { get; set; }

        public string Name { get; set; }

        public int Id { get; private set; }

        public ulong Index { get; set; }

        public TaskModel(string name, bool state, ulong index)
        {
            State = state;
            Name = name;
            Id = GetHashCode();
            Index = index;
        }

        public override bool Equals(object obj)
        {
            return GetHashCode() == obj.GetHashCode();
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
