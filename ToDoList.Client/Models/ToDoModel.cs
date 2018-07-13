using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDoList.Client
{
    class ToDoModel
    {
        public HashSet<TaskModel> ItemsData = new HashSet<TaskModel>();

        public bool TryAddItem(TaskModel task)
        {
            if (ItemsData.Contains(task))
                return false;

            ItemsData.Add(task);
            return true;
        }


        public bool TryDeleteItem(TaskModel task)
        {
            if (!ItemsData.Contains(task))
                return false;

            ItemsData.Remove(task);
            return true;
        }
    }
}
