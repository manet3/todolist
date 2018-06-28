using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ToDoList.Server.Models;

namespace ToDoList.Server.Controllers
{
    public class TasksController : ApiController
    {

        public class NameComparer<T> : IEqualityComparer<TaskModel>
        {
            public bool Equals(TaskModel x, TaskModel y)
            {
                return x.Name.Equals(y.Name);
            }

            public int GetHashCode(TaskModel obj) => obj.GetHashCode();
        }

        public TasksController()
        {
        }

        [ActionName("List")]
        public IEnumerable<TaskModel> Get()
        {
            IEnumerable<TaskModel> dbItems = new TaskModel[0];
            ToDoDBModel.UsingDb(()=>ToDoDBModel.GetDBItems(ref dbItems));
            return dbItems;
        }

        public TaskModel Get(int id)
        {
            TaskModel item = null;
            ToDoDBModel.UsingDb(()=> ToDoDBModel.GetDbItem(id, ref item));
            return item;
        }

        [ActionName("Add")]
        public void Post(TaskModel task)
        {
            ToDoDBModel.UsingDb(()=> ToDoDBModel.AddToDB(task));
        }

        [AcceptVerbs("PUT", "PATCH")]
        [ActionName("Change")]
        public void EditTask(IEnumerable<TaskModel> new_tasks)
        {
            //var chgTable = new_tasks
            //    .Zip(new_tasks, (k, v) => new { key = k.Id, value = v })
            //    .ToDictionary(t => t.key,  t=> t.value);
            //var toAdd = new List<TaskModel>();
            //foreach (var task in _tasks)
            //    if (chgTable.ContainsKey(task.Id))
            //        task.State = chgTable[task.Id].State;
            //_tasks.AddRange(toAdd);
        }

        [ActionName("Remove")]
        public void DeleteTask(int id)
        {
            ToDoDBModel.UsingDb(() => ToDoDBModel.RemoveDBItem(id));
        }
    }
}
