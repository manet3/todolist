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
        private IToDoItemsRepository _repository;

        public ToDoController(IToDoItemsRepository repository)
        {
            _repository = repository;
            _repository.ConnectStorage();
        }

        [HttpGet]
        public IEnumerable<ToDoItem> List()
        {
            ThrowIfLiteDbNotExists();

            var result = _repository.List();

            if (result.IsFailure)
                throw GetExceptionWith(result.Error, HttpStatusCode.InternalServerError);

            return Mapper.Map<IEnumerable<ToDoItem>>(result.Value);
        }

        [HttpPost]
        public void Add(ToDoItem item)
        {
            ThrowIfLiteDbNotExists();

            OnNullThrowArgumentException(item);

            var result = _repository.Add(Mapper.Map<ItemDbModel>(item));

            if (result.IsFailure)
                throw GetExceptionWith(result.Error, HttpStatusCode.InternalServerError);
        }

        [HttpPut, HttpPatch]
        public void Change(ToDoItem item)
        {
            ThrowIfLiteDbNotExists();

            OnNullThrowArgumentException(item);

            var result = _repository.UpdateItem(Mapper.Map<ItemDbModel>(item));

            if (result.IsFailure)
                throw GetExceptionWith(result.Error, HttpStatusCode.InternalServerError);
        }

        [HttpDelete]
        public void Delete(string id)
        {
            //ThrowIfLiteDbNotExists();

            //OnNullThrowArgumentException(id);

            //var item = ToDoItem.Parse(id);
            //var result = _repository.DeleteById(item.Id, item.Timestamp);

            //if (result.IsFailure)
            //    throw GetExceptionWith(result.Error, HttpStatusCode.InternalServerError);
        }

        private void OnNullThrowArgumentException(object argument)
        {
            if (argument == null)
                throw GetExceptionWith("Failed to get client data.", HttpStatusCode.BadRequest);
        }

        private void ThrowIfLiteDbNotExists()
        {
            if (_repository.StorageConnection.IsFailure)
                throw GetExceptionWith(_repository.StorageConnection.Error, HttpStatusCode.ServiceUnavailable);
        }

        private HttpResponseException GetExceptionWith(string reasonForFailure, HttpStatusCode statusCode)
            => new HttpResponseException(new HttpResponseMessage(statusCode)
            { ReasonPhrase = reasonForFailure.Replace("\r\n", " | ") });
    }
}
