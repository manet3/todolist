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
            var result = ItemsDbProvider.GetDBItems();

            if (result.IsFailure)
                throw GetExceptionWith(result.Error, HttpStatusCode.InternalServerError);

            return Mapper.Map<IEnumerable<ToDoItem>>(result.Value);
        }

        [HttpPost]
        public void Add(ToDoItem item)
        {
            if (item == null)
                throw GetExceptionWith();

            var result = ItemsDbProvider.AddToDB(Mapper.Map<ItemDbModel>(item));

            if (result.IsFailure)
                throw GetExceptionWith(result.Error, HttpStatusCode.InternalServerError);
        }

        [HttpPut, HttpPatch]
        public void Rewrite(IEnumerable<ToDoItem> new_items)
        {
            if (new_items == null)
                throw GetExceptionWith();

            var result = ItemsDbProvider.DBRewrite(Mapper.Map<IEnumerable<ItemDbModel>>(new_items));

            if (result.IsFailure)
                throw GetExceptionWith(result.Error, HttpStatusCode.InternalServerError);
        }

        [HttpPatch]
        public void Change(ToDoItem item)
        {
            if (item == null)
                throw GetExceptionWith();

            var result = ItemsDbProvider.AddToDB(Mapper.Map<ItemDbModel>(item));

            if (result.IsFailure)
                throw GetExceptionWith(result.Error, HttpStatusCode.InternalServerError);

        }


        [HttpDelete]
        public void Delete(string name)
        {
            if (name == null)
                throw GetExceptionWith();

            var result = ItemsDbProvider.TryRemoveDBItem(name);

            if (result.IsFailure)
                throw GetExceptionWith(result.Error, HttpStatusCode.InternalServerError);
        }

        private HttpResponseException GetExceptionWith(
            string reasonForFailure = "Failed to get client data.",
            HttpStatusCode statusCode = HttpStatusCode.BadRequest)
            => new HttpResponseException(new HttpResponseMessage(statusCode) { ReasonPhrase = reasonForFailure });
    }
}
