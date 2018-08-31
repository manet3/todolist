using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using ToDoList.Server.Database.POCOs;

namespace ToDoList.Server.Database
{
    public interface IStorageRepository : IDisposable
    {
        void ConfigureStorage();
    }

    public interface IListsRepository : IStorageRepository
    {
        Result Add(ToDoListPoco list);

        Result<IEnumerable<ToDoListPoco>> Get();

        Result DeleteById(ulong id, DateTime timestamp);
    }

    public interface IItemsRepository : IStorageRepository
    {
        Result Add(ToDoItemPoco list);

        Result UpdateItem(ToDoItemPoco item);

        Result DeleteById(ulong id, DateTime timestamp);
    }
}
