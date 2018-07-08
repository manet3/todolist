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
        public HashSet<TaskModel> _tasks;

        public async Task<HashSet<TaskModel>> GetTasks()
        {
            _tasks = await Synchronisator.GetTasksAsync();
            return _tasks;
        }

        public bool TryAddItem(TaskModel task)
        {
            if (_tasks.Contains(task))
                return false;

            Update(Synchronisator.AddAsync, task);

            return true;
        }


        public bool TryDeleteItem(TaskModel task)
        {
            if (!_tasks.Contains(task))
                return false;

            Update(Synchronisator.DeleteItemAsync, task);

            return true;
        }

        private void Update(Func<TaskModel, Task> sendActionAsync, TaskModel task)
        {
            //uploading the item
            if (!sendActionAsync(task).IsCompleted)
                Synchronisator.LoadingStartedInvoke();
        }

    }
}
