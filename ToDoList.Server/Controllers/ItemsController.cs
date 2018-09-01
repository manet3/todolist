using System.Web.Http;
using ToDoList.Server.Database;
using ToDoList.Server.Database.POCOs;
using AutoMapper;
using ToDoList.Shared;

namespace ToDoList.Server.Controllers
{
    public class ItemsController : ApiController
    {
        private IItemsRepository _repository;

        public ItemsController(IItemsRepository repository)
        {
            _repository = repository;
            _repository.ConfigureStorage();
            Mapper.Initialize(m => m.CreateMap<ToDoItem, ItemPoco>());
        }

        [HttpPost]
        public void Add(ToDoItem item)
        {
            ControllerExceptionHandler.ThrowIfNullArgument(item);

            var result = _repository.Add(Mapper.Map<ItemPoco>(item));

            ControllerExceptionHandler.ThrowIfFailure(result);
        }

        [HttpPut, HttpPatch]
        public void Change(ToDoItem item)
        {
            ControllerExceptionHandler.ThrowIfNullArgument(item);

            var result = _repository.Update(Mapper.Map<ItemPoco>(item));

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
