using System.Collections.Generic;
using System.Web.Http;
using ToDoList.Server.Database;
using ToDoList.Server.Database.Models;
using AutoMapper;
using ToDoList.Shared;
using System.Net;
using System.Net.Http;

namespace ToDoList.Server.Controllers
{
    public class ToDoController : ApiController
    {
        [HttpGet]
        public IEnumerable<ToDoItem> List()
        {
            if (!ItemsDbProvider.TryGetDBItems(out IEnumerable<ItemDbModel> dbItems))
                throw GetExceptionWith("Could not get items from DB.");

            return Mapper.Map<IEnumerable<ToDoItem>>(dbItems);
        }

        [HttpPost]
        public void Add(ToDoItem task)
        {
            if (task == null)
                throw GetExceptionWith("Failed to get client data.", HttpStatusCode.BadRequest);
            ItemsDbProvider.AddToDB(Mapper.Map<ItemDbModel>(task));
        }

        [HttpPut, HttpPatch]
        public void Change(IEnumerable<ToDoItem> new_tasks)
        {
            if (new_tasks == null)
                throw GetExceptionWith("Failed to get client data.", HttpStatusCode.BadRequest);
            ItemsDbProvider.UpdateDB(Mapper.Map<IEnumerable<ItemDbModel>>(new_tasks));
        }

        [HttpDelete]
        public void Delete(string name)
        {
            if(!ItemsDbProvider.TryRemoveDBItem(name))
                throw GetExceptionWith("Cannot delete item.");

        }

        private HttpResponseException GetExceptionWith(string reasonForFailure, 
            HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
            => new HttpResponseException(new HttpResponseMessage(statusCode) { ReasonPhrase = reasonForFailure });
    }
}
