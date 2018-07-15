using ServiceStack.DataAnnotations;
using System.Runtime.Serialization;

namespace ToDoList.Shared
{
    [DataContract]
    public class ToDoItem
    {
        [DataMember, AutoIncrement]
        public int Id { get; set; }
        [DataMember, Required, Unique]
        public string Name { get; set; }
        [DataMember]
        public bool IsChecked { get; set; }
        [DataMember]
        public ulong Index { get; set; }

        public ToDoItem(string text, bool isChecked = false, ulong index = 0)
        {
            Name = text;
            IsChecked = isChecked;
            Index = index;
        }

        public override bool Equals(object obj)
        {
            return obj is ToDoItem && Name.Equals(((ToDoItem)obj).Name);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
