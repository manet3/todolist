using ToDoList.Server.Database.POCOs;
using ServiceStack.OrmLite;
using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ToDoList.Server.Database
{
    public class ListsRepository : RepositoryBase, IListsRepository
    {
        public override void ConfigureStorage()
            => ExceptionsHandler.DbConfiguration = CreateTablesIfNot(typeof(ListPoco), typeof(ItemPoco));

        public Result<IEnumerable<ListPoco>> Get()
            => ExceptionsHandler.DbExceptionsHandle(
                () => Result.Ok(
                    DbConn.Select<ListPoco>().Select(list =>
                    {
                        list.ToDoItems = DbConn.Select<ItemPoco>(item => item.ListPocoId == list.Id).ToArray();
                        return list;
                    })),
                "Failed to read DB data.");

        public Result Add(ListPoco list)
            => ExceptionsHandler.DbExceptionsHandle(() =>
            {
                var existingList = DbConn.Single<ListPoco>(x => x.Name == list.Name);
                if (existingList != null)
                    MergeListIntoDb(list, existingList.Id);
                else
                    DbConn.Save(list, references: true);

                return Result.Ok();
            },
                "Failed to add ToDoList.");

        private void MergeListIntoDb(ListPoco source, ulong targetId)
        {
            foreach (var item in source.ToDoItems)
            {
                item.ListPocoId = targetId;
                var existingItem = DbConn.Single<ItemPoco>(x => x.Name == item.Name);

                if (existingItem == null)
                    DbConn.Insert(item);

                if (existingItem != null && existingItem.Timestamp < item.Timestamp)
                {
                    item.Id = existingItem.Id;
                    UpdateItemById(item);
                }
            }
        }

        public Result DeleteById(ulong id, DateTime timestamp)
            => ExceptionsHandler.DbExceptionsHandle(() =>
            {
                var dbListGot = GetById<ListPoco>(id);

                if (dbListGot.IsSuccess
                && timestamp >= dbListGot.Value.Timestamp
                && !DbConn.Exists<ItemPoco>(x => x.Timestamp > timestamp))
                {
                    DbConn.Delete<ListPoco>(x => x.Id == id);
                    DbConn.Delete<ItemPoco>(x => x.ListPocoId == id);
                }

                return dbListGot;
            },
                "Failed to delete list.");
    }
}