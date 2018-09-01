using ToDoList.Server.Database.POCOs;
using CSharpFunctionalExtensions;
using ServiceStack;
using ServiceStack.OrmLite;
using System;

namespace ToDoList.Server.Database
{
    public class ItemsRepository : RepositoryBase, IItemsRepository
    {
        public override void ConfigureStorage()
            => ExceptionsHandler.DbConfiguration = CreateTablesIfNot(typeof(ItemPoco));

        public Result Add(ItemPoco item)
            => ExceptionsHandler.DbExceptionsHandle(

                () => DbConn.Exists<ItemPoco>(new { Name = item.Name })
                ? Result.Fail("This item already exists.")
                : Result.Ok(DbConn.Insert(item)),

                "Could not add item to DB.");

        public Result Update(ItemPoco item)
            => ExceptionsHandler.DbExceptionsHandle(() =>
            {
                var dbItemGot = GetById<ItemPoco>(item.Id);

                if (dbItemGot.IsSuccess && item.Timestamp >= dbItemGot.Value.Timestamp)
                    UpdateItemById(item);

                return dbItemGot;
            },
                "Failed to update item.");

        public Result DeleteById(ulong id, DateTime timestamp)
            => ExceptionsHandler.DbExceptionsHandle(() =>
            {
                var dbItemGot = GetById<ItemPoco>(id);

                if (dbItemGot.IsSuccess && timestamp >= dbItemGot.Value.Timestamp)
                    DbConn.Delete<ItemPoco>(x => x.Id == id);

                return dbItemGot;
            },
                "Failed to delete item.");
    }
}