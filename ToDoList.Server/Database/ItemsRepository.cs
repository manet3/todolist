using ToDoList.Server.Database.POCOs;
using ToDoList.Server.Database.Services;
using CSharpFunctionalExtensions;
using ServiceStack;
using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;

namespace ToDoList.Server.Database
{
    public class ItemsRepository : RepositoryBase, IItemsRepository
    {
        public override void ConfigureStorage()
            => ExceptionsHandler.DbConfiguration = CreateTablesIfNot(typeof(ToDoItemPoco));

        public Result Add(ToDoItemPoco item)
            => ExceptionsHandler.DbExceptionsHandle(

                () => DbConn.Exists<ToDoItemPoco>(new { Name = item.Name })
                ? Result.Fail("This item already exists.")
                : Result.Ok(DbConn.Insert(item)),

                "Could not add item to DB.");

        public Result<IEnumerable<ToDoItemPoco>> List()
            => ExceptionsHandler.DbExceptionsHandle(

                () => Result.Ok<IEnumerable<ToDoItemPoco>>(DbConn.Select<ToDoItemPoco>()),

                "Failed to get DB table");

        public Result UpdateItem(ToDoItemPoco item)
            => ExceptionsHandler.DbExceptionsHandle(() =>
            {
                var dbItem = DbConn.Single<ToDoItemPoco>((x) => x.Name == item.Name);

                if (dbItem == null)
                    return Result.Fail("Item not found");

                if (item.Timestamp >= dbItem.Timestamp)
                    DbConn.Update<ToDoItemPoco>(
                        new
                        {
                            IsChecked = item.IsChecked,
                            Timestamp = item.Timestamp
                        },
                        where: x => x.Name == item.Name);

                return Result.Ok();
            },
                "Failed to update item");

        public Result DeleteById(ulong id, DateTime timestamp)
            => ExceptionsHandler.DbExceptionsHandle(() =>
            {
                var dbItem = DbConn.Single<ToDoItemPoco>(x => x.Id == id);

                if (dbItem == null)
                    return Result.Fail("Item not found");

                if (timestamp >= dbItem.Timestamp)
                    DbConn.Delete<ToDoItemPoco>(x => x.Id == id);

                return Result.Ok();
            },
                "Failed to delete item");
    }
}