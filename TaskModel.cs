using ServStack =  ServiceStack.DataAnnotations;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace ToDoList
{
    [DataContract]
    public class TaskModel
    {
        [DataMember]
        public bool State { get; set; }

        [Required]
        [DataMember(Order = 1)]
        public string Name { get; set; }

        [DataMember(Order = 0)]
        public int Id { get; private set; }

        [DataMember]
        public ulong Index { get; set; }

        public TaskModel(string name, bool state, ulong index)
        {
            State = state;
            Name = name;
            Id = GetHashCode();
            Index = index;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
