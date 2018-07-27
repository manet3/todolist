using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDoList.Client.DataServices;
using ToDoList.Shared;

namespace ToDoList.Client.Models
{
    class ToDoModel
    {
        private bool _isSynchronised;

        private Synchronisator _sync;

        public ToDoModel()
            => _sync = Synchronisator.SyncInit();

        public async Task<Result> AddAsync(ToDoItem item)
            => HandleRes(await _sync.AddAsync(item));

        public async Task<Result> DeleteAsync(ToDoItem item) 
            => HandleRes(await _sync.DeleteItemAsync(item));

        public async Task<Result> UpdateAsync(ToDoItem item) 
            => HandleRes(await _sync.ChangeAsync(item));

        public async Task<Result> UpdateAllAsync(IEnumerable<ToDoItem> items) 
            => HandleRes(await _sync.ChangeAsync(items));

        public async Task<Result<IEnumerable<ToDoItem>>> GetAsync()
            => HandleRes(await _sync.GetTasksAsync());

        private Result<T> HandleRes<T>(Result<T> res)
        {
            _isSynchronised = !res.IsFailure;
            return res;
        }

        public void SaveIfNotSynchronised()
        {
            //Call writing to local json
            if (!_isSynchronised)
                throw new NotImplementedException();
        }
    }
}
