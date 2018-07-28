using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ToDoList.Shared;

namespace ToDoList.Client.DataServices
{
    class DataServicesManager
    {
        private bool _isSynchronised;

        private Synchronisator _sync;

        public DataServicesManager()
            => _sync = Synchronisator.SyncInit();

        public async Task<Result<HttpResponseMessage>> AddAsync(ToDoItem item)
            => HandleRes(await _sync.SendRequestAsync(item, ApiAction.Add));

        public async Task<Result<HttpResponseMessage>> DeleteByNameAsync(string name)
            => HandleRes(await _sync.SendRequestAsync(name, ApiAction.Delete));

        public async Task<Result<HttpResponseMessage>> UpdateAsync(ToDoItem item)
            => HandleRes(await _sync.SendRequestAsync(item, ApiAction.Change));

        public async Task<Result<HttpResponseMessage>> UpdateAllAsync(IEnumerable<ToDoItem> items)
            => HandleRes(await _sync.SendRequestAsync(items, ApiAction.Rewrite));

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
