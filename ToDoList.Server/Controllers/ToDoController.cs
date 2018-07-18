using System.Collections.Generic;
using System.Web.Http;
using ToDoList.Server.Database;
using ToDoList.Server.Database.Models;
using AutoMapper;
using ToDoList.Shared;
using System.Net;
using System.Net.Http;
using CSharpFunctionalExtensions;

namespace ToDoList.Server.Controllers
{
    public class ToDoController : ApiController
    {
        [HttpGet]
        public IEnumerable<ToDoItem> List()
        {
            ResultCheck(ItemsDbProvider.CreateTableIfNotExists());

            var result = ItemsDbProvider.GetDBItems();
            ResultCheck(result);

            return Mapper.Map<IEnumerable<ToDoItem>>(result.Value);
        }

        [HttpPost]
        public void Add(ToDoItem item)
        {
            NullCheck(item);
            ResultCheck(ItemsDbProvider.CreateTableIfNotExists());
            ItemsDbProvider.AddToDB(Mapper.Map<ItemDbModel>(item));
        }

        [HttpPut, HttpPatch]
        public void Change(IEnumerable<ToDoItem> new_tasks)
        {
            NullCheck(new_tasks);
            ResultCheck(ItemsDbProvider.CreateTableIfNotExists());
            ResultCheck(ItemsDbProvider.DBRewrite(Mapper.Map<IEnumerable<ItemDbModel>>(new_tasks)));
        }

        [HttpPut, HttpPatch]
        public void Change(ToDoItem item)
        {
            NullCheck(item);
            ResultCheck(ItemsDbProvider.CreateTableIfNotExists());
            ResultCheck(ItemsDbProvider.AddToDB(Mapper.Map<ItemDbModel>(item)));
        }


        [HttpDelete]
        public void Delete(string name)
        {
            NullCheck(name);
            ResultCheck(ItemsDbProvider.CreateTableIfNotExists());
            ResultCheck(ItemsDbProvider.TryRemoveDBItem(name));
        }

        private void NullCheck(object item)
        {
            if (item == null)
                throw GetExceptionWith("Failed to get client data.");
        }

        private void ResultCheck(Result result)
        {
            if (result.IsFailure)
                throw GetExceptionWith(result.Error, HttpStatusCode.InternalServerError);
        }

        private HttpResponseException GetExceptionWith(string reasonForFailure, 
            HttpStatusCode statusCode = HttpStatusCode.BadRequest)
            => new HttpResponseException(new HttpResponseMessage(statusCode) { ReasonPhrase = reasonForFailure });
    }
}
