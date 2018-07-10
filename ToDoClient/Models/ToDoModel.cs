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
        public HashSet<TaskModel> ItamsData;

        private Action _unfinishedAction;

        public event Action GotItems;

        public ToDoModel()
        {
            Synchronisator.SynchChanged += ActFinishedCheck;
        }

        private void ActFinishedCheck(SyncState state)
        {
            if (state != SyncState.Failed)
                _unfinishedAction = null;
        }

        public async Task GetItems()
        {
            _unfinishedAction = () =>  GetItems();

            var itemTask = Synchronisator.GetTasksAsync();

            if (!itemTask.IsCompleted)
            {
                Synchronisator.LoadingStartedInvoke();
                ItamsData = await itemTask;
            }
            else ItamsData = itemTask.Result;


            if (!Synchronisator.IsSyncSuccessful)
            {
                ItamsData = ProgressSaver<HashSet<TaskModel>>.TryGetSessions().Last();
                GotItems();
            }

        }

        public void Retry() => _unfinishedAction();

        public bool TryAddItem(TaskModel task)
        {
            if (ItamsData.Contains(task))
                return false;

            ItamsData.Add(task);

            Update(Synchronisator.AddAsync, task);

            return true;
        }


        public bool TryDeleteItem(TaskModel task)
        {
            if (!ItamsData.Contains(task))
                return false;

            ItamsData.Remove(task);

            Update(Synchronisator.DeleteItemAsync, task);

            return true;
        }

        public void CheckSaveProgress()
        {
            if (!Synchronisator.IsSyncSuccessful)
                ProgressSaver<HashSet<TaskModel>>.SaveCurrentSession(ItamsData);
        }

        private void Update(Func<TaskModel, Task> sendActionAsync, TaskModel task)
        {
            //will try to update by scedule
            _unfinishedAction = UpdateAll;
            if (!Synchronisator.IsSyncSuccessful) return;

            //uploading the item
            if (!sendActionAsync(task).IsCompleted)
                Synchronisator.LoadingStartedInvoke();
        }

        private void UpdateAll()
        {
            if (!Synchronisator.AddAsync(ItamsData).IsCompleted)
                Synchronisator.LoadingStartedInvoke();
        }

    }
}
