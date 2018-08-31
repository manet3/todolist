using CSharpFunctionalExtensions;
using ServiceStack.OrmLite;
using System;
using System.Data;
using ToDoList.Server.Database.Services;

namespace ToDoList.Server.Database
{
    public abstract class RepositoryBase : IStorageRepository
    {
        protected IDbConnection DbConn;

        protected DbExceptionsHandler ExceptionsHandler;

        public RepositoryBase()
        {
            DbConn = DbConnectionDistributor.DbConnection;
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

        public void Dispose()
        {
            DbConn?.Dispose();
            DbConn = null;
        }
    }
}