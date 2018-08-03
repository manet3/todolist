using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using ToDoList.Server.Database.Models;

namespace ToDoList.Server.Database
{
    /// <returns>
    /// Methods return Result.Fail if operation failed
    /// </returns>
    public interface IToDoItemsRepository : IDisposable
    {
        Result StorageConnection { get; }

        Result ConnectStorage();

        /// <returns> Fail if such item already exists</returns>
        Result Add(ItemDbModel item);

        /// <param name="name"> Deletes item by unique content </param>
        /// <returns> Fail if no such name found </returns>
        Result DeleteByName(string name, DateTime timestamp);

        Result<IEnumerable<ItemDbModel>> List();

        Result UpdateAllForce(IEnumerable<ItemDbModel> new_items);

        Result UpdateItem(ItemDbModel item);
    }
}
