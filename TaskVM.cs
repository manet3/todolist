using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ToDoList.Client
{
    class TaskVM
    {
        public TaskModel Model;

        public static event EventHandler<bool> CheckedChanged;

        public bool IsDone
        {
            get => Model.State;
            set
            {
                Model.State = value;
                CheckedChanged?.Invoke(this, value);
            }
        }

        public string Content { get; set; }

        private TaskVM()
        {
        }

        public TaskVM(string content, bool state):this()
        {
            Content = content;
            Model = new TaskModel(content, state);
        }

        public TaskVM(TaskModel model):this()
        {
            Content = model.Name;
            Model = model;
        }


    }
}
