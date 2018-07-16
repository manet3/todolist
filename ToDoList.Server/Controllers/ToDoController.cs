using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ToDoList.Server.Database;
using ToDoList.Shared;

namespace ToDoList.Server.Controllers
{
    public class ToDoController : ApiController
    {
        [ActionName("List")]
        public IEnumerable<ToDoItem> Get()
        {
            ItemsDbProvider.GetDBItems(out IEnumerable<ToDoItem> dbItems);
            return dbItems;
        }

        [ActionName("Add")]
        public void Post(ToDoItem task)
        {
            ItemsDbProvider.AddToDB(task);
        }

        [HttpPut, HttpPatch]
        [ActionName("Change")]
        public void RewriteItems(IEnumerable<ToDoItem> new_tasks)
        {
            ItemsDbProvider.UpdateDB(new_tasks);
        }

        public void Delete(string name)
        {
            ItemsDbProvider.RemoveDBItem(name);
        }

    }
}
