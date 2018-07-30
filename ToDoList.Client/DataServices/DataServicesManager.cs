using CSharpFunctionalExtensions;
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

        private LocalStorage _saver;

        public DataServicesManager()
        {
            _sync = Synchronisator.SyncInit();
            _saver = LocalStorage.InitStorage();
        }

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

        public IEnumerable<ToDoItem> GetLocal()
            => _saver.GetSessions();

        private Result<T> HandleRes<T>(Result<T> res)
        {
            _isSynchronised = !res.IsFailure;
            return res;
        }

        public void SaveIfNotSynchronised(IEnumerable<ToDoItem> items)
        {
            if (!_isSynchronised)
                _saver.SaveSession(items);
        }
    }
}
