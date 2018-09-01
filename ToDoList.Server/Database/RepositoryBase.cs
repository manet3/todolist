using CSharpFunctionalExtensions;
using ServiceStack.OrmLite;
using System;
using System.Data;
using ToDoList.Server.Database.POCOs;
using ToDoList.Server.Database.Services;

namespace ToDoList.Server.Database
{
    public abstract class RepositoryBase : IStorageRepository
    {
        protected IDbConnection DbConn;

        protected DbExceptionsHandler ExceptionsHandler;

        public RepositoryBase()
        {
            DbConn = DbConnectionDistributor.OpenConnection();
            ExceptionsHandler = new DbExceptionsHandler
            {
                DbConfiguration = DbConnectionDistributor.DbConnectingResult
            };
        }

        public abstract void ConfigureStorage();

        protected Result CreateTablesIfNot(params Type[] types)
            => ExceptionsHandler.DbExceptionsHandle(() =>
            {
                foreach (var type in types)
                    DbConn.CreateTableIfNotExists(type);
                return Result.Ok();
            },
                "Failed to create Db table");

        protected Result<T> GetById<T>(ulong id) where T : PocoCommon
        {
            var dbItem = DbConn.Single<T>(x => x.Id == id);

            return dbItem == null ? Result.Fail<T>("Item not found") : Result.Ok(dbItem);
        }

        protected void UpdateItemById(ItemPoco item)
        {
            DbConn.Update<ItemPoco>(new
            {
                IsChecked = item.IsChecked,
                Timestamp = item.Timestamp
            },
            where: x => x.Id == item.Id);
        }

        public void Dispose() => DbConnectionDistributor.CloseConnection();
    }
}