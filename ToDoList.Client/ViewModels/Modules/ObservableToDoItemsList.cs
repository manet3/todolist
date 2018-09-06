using System.Collections.Generic;
using ToDoList.Shared;

namespace ToDoList.Client.ViewModels.Modules
{
    public class ObservableToDoItemsList : ObservableHashSet<ToDoItem>
    {
        private ToDoItemsList _baseList;

        public string Name { get => _baseList?.Name; }

        public List<ToDoItem> SelectedItems { get; set; }

        public ObservableToDoItemsList(ToDoItemsList baseList)
            : base(baseList.Items ?? new List<ToDoItem>())
            => _baseList = baseList;

        public override bool Equals(object obj)
            => obj is ObservableToDoItemsList list && _baseList.Equals(list._baseList);

        public override int GetHashCode()
            => _baseList.GetHashCode();

        public static implicit operator ToDoItemsList(ObservableToDoItemsList interactiveToDoList)
        {
            var list = interactiveToDoList._baseList;
            list.Items = interactiveToDoList.Items;

            return list;
        }
    }
}
