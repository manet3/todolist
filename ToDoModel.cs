using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDoList.Client
{
    class ToDoModel
    {
        public HashSet<TaskModel> Tasks;

        public ToDoModel()
        {
            Tasks = Synchronisator.GetTasks();
        }

        public void AddItem(TaskModel task)
        {
            Synchronisator.Add(task);
        }

        public void DeleteItem(TaskModel task)
        {
            Synchronisator.DeleteTask(task);
        }
    }
}
