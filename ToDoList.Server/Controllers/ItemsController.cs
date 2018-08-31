using System.Web.Http;
using ToDoList.Server.Database;
using ToDoList.Server.Database.POCOs;
using AutoMapper;
using ToDoList.Shared;

namespace ToDoList.Server.Controllers
{
    public class ToDoItemsController : ApiController
    {
        private IItemsRepository _repository;

        public ToDoItemsController(IItemsRepository repository)
        {
            _repository = repository;
            _repository.ConfigureStorage();
            Mapper.Initialize(m => m.CreateMap<ToDoItem, ToDoItemPoco>());
        }

        [HttpPost]
        public void Add(ToDoItem item)
        {
            ControllerExceptionHandler.ThrowIfNullArgument(item);

            var result = _repository.Add(Mapper.Map<ToDoItemPoco>(item));

            ControllerExceptionHandler.ThrowIfFailure(result);
        }

        [HttpPut, HttpPatch]
        public void Change(ToDoItem item)
        {
            ControllerExceptionHandler.ThrowIfNullArgument(item);

            var result = _repository.UpdateItem(Mapper.Map<ToDoItemPoco>(item));

            ControllerExceptionHandler.ThrowIfFailure(result);
        }

        public void Delete(string id)
        {
            ControllerExceptionHandler.ThrowIfNullArgument(id);

            var item = SyncEntityUrlStringRepresentation.Parse(id);
            var result = _repository.DeleteById(item.Id, item.Timestamp);

            ControllerExceptionHandler.ThrowIfFailure(result);
        }
    }
}
