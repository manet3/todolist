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
            ThrowIfDbNotExists();

            var result = ItemsDbProvider.GetDBItems();

            if (result.IsFailure)
                throw GetExceptionWith(result.Error, HttpStatusCode.InternalServerError);

            return Mapper.Map<IEnumerable<ToDoItem>>(result.Value);
        }

        [HttpPost]
        public void Add(ToDoItem item)
        {
            ThrowIfDbNotExists();

            OnNullThrowArgumentException(item);

            var result = ItemsDbProvider.AddToDB(Mapper.Map<ItemDbModel>(item));

            if (result.IsFailure)
                throw GetExceptionWith(result.Error, HttpStatusCode.InternalServerError);
        }

        [HttpPut, HttpPatch]
        public void Rewrite(IEnumerable<ToDoItem> new_items)
        {
            ThrowIfDbNotExists();

            OnNullThrowArgumentException(new_items);

            var result = ItemsDbProvider.DBRewrite(Mapper.Map<IEnumerable<ItemDbModel>>(new_items));

            if (result.IsFailure)
                throw GetExceptionWith(result.Error, HttpStatusCode.InternalServerError);
        }

        [HttpPut, HttpPatch]
        public void Change(ToDoItem item)
        {
            ThrowIfDbNotExists();

            OnNullThrowArgumentException(item);

            var result = ItemsDbProvider.DBUpdateItem(Mapper.Map<ItemDbModel>(item));

            if (result.IsFailure)
                throw GetExceptionWith(result.Error, HttpStatusCode.InternalServerError);

        }

        [HttpDelete]
        public void Delete(string id)
        {
            ThrowIfDbNotExists();

            OnNullThrowArgumentException(id);

            var result = ItemsDbProvider.TryRemoveDBItem(id);

            if (result.IsFailure)
                throw GetExceptionWith(result.Error, HttpStatusCode.InternalServerError);
        }

        private void OnNullThrowArgumentException(object argument)
        {
            if (argument == null)
                throw GetExceptionWith("Failed to get client data.", HttpStatusCode.BadRequest);
        }

        private void ThrowIfDbNotExists()
        {
            if (ItemsDbProvider.TableCreationResult.IsFailure)
                throw GetExceptionWith(ItemsDbProvider.TableCreationResult.Error, HttpStatusCode.ServiceUnavailable);
        }

        private HttpResponseException GetExceptionWith(string reasonForFailure, HttpStatusCode statusCode)
            => new HttpResponseException(new HttpResponseMessage(statusCode)
            { ReasonPhrase = reasonForFailure.Replace("\r\n", " | ") });
    }
}
