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
        Result Add(ListPoco list);

        Result<IEnumerable<ListPoco>> Get();

        Result DeleteById(ulong id, DateTime timestamp);
    }

    public interface IItemsRepository : IStorageRepository
    {
        Result Add(ItemPoco list);

        Result Update(ItemPoco item);

        Result DeleteById(ulong id, DateTime timestamp);
    }
}
