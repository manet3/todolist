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

            if (!Synchronisator.IsSynchronized)
                _tasks = ProgressSaver<HashSet<TaskModel>>.TryGetSessions().Last();

            return _tasks;
        }

        public bool TryAddItem(TaskModel task)
        {
            if (_tasks.Contains(task))
                return false;

            _tasks.Add(task);

            Update(Synchronisator.AddAsync, task);

            return true;
        }


        public bool TryDeleteItem(TaskModel task)
        {
            if (!_tasks.Contains(task))
                return false;

            _tasks.Remove(task);

            Update(Synchronisator.DeleteItemAsync, task);

            return true;
        }

        public void CheckSaveProgress()
        {
            if (!Synchronisator.IsSynchronized)
                ProgressSaver<HashSet<TaskModel>>.SaveCurrentSession(_tasks);
        }

        private void Update(Func<TaskModel, Task> sendActionAsync, TaskModel task)
        {
            //uploading the item
            if (!sendActionAsync(task).IsCompleted)
                Synchronisator.LoadingStartedInvoke();
        }

    }
}
