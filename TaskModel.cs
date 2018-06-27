using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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

        public TaskModel(string name, bool state)
        {
            State = state;
            Name = name;
            Id = GetHashCode();
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
