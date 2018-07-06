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

        public async Task AddItemAsync(TaskModel task)
        {
            await Synchronisator.Add(task);
        }

        public async Task DeleteItemAsync(TaskModel task)
        {
            await Synchronisator.DeleteItemAsync(task);
        }

    }
}
