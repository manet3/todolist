using System.Web.Http;
using ToDoList.Server.Database;
using ToDoList.Server.Database.POCOs;
using AutoMapper;
using ToDoList.Shared;
using System.Collections.Generic;

namespace ToDoList.Server.Controllers
{
    public class ListsController : ApiController
    {
        private IListsRepository _repository;

        public ListsController(IListsRepository repository)
        {
            _repository = repository;
            _repository.ConfigureStorage();
            Mapper.Initialize(m => m.CreateMap<ToDoItemsList, ListPoco>());
        }

        public IEnumerable<ToDoItemsList> GetAll()
        {
            var result = _repository.Get();

            ControllerExceptionHandler.ThrowIfFailure(result);

            return Mapper.Map<IEnumerable<ToDoItemsList>>(result.Value);
        }

        [HttpPost]
        public void Add(ToDoItemsList list)
        {
            ControllerExceptionHandler.ThrowIfNullArgument(list);

            var result = _repository.Add(Mapper.Map<ListPoco>(list));

            ControllerExceptionHandler.ThrowIfFailure(result);
        }

        public void Delete(string id)
        {
            ControllerExceptionHandler.ThrowIfNullArgument(id);

            var list = SyncEntityUrlStringRepresentation.Parse(id);
            var result = _repository.DeleteById(list.Id, list.Timestamp);

            ControllerExceptionHandler.ThrowIfFailure(result);

        }
    }
}
