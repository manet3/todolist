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
        public HashSet<TaskModel> Tasks;

        public event Action GotItems;

        public void ReadData()
        {
            var readThread = new Thread(() =>
            {
                Tasks = Synchronisator.GetTasks();
                GotItems?.Invoke();
                Thread.CurrentThread.Abort();
            })
            { IsBackground = true};

            readThread.Start();
        }

        public void AddItem(TaskModel task)
        {
            SendItem(Synchronisator.Add, task);
        }

        public void DeleteItem(TaskModel task)
        {
            SendItem(Synchronisator.DeleteTask, task);
        }

        public void SendItem(Action<TaskModel> action, TaskModel task)
        {
            var sendThread = new Thread(() =>
            {
                action(task);
                Thread.CurrentThread.Abort();
            });
            sendThread.Start();
        }
    }
}
