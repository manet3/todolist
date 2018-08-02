using CSharpFunctionalExtensions;
using ServiceStack;
using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Data;
using ToDoList.Server.Database.Models;

namespace ToDoList.Server.Database
{
    public class ToDoItemsLiteRepository : IToDoItemsRepository
    {
        private DbExceptionHandler _exceptionHandle;

        private IDbConnection _dbConn;

        public Result StorageConnection { get; private set; }
            = Result.Fail("No DB connection.");

        public ToDoItemsLiteRepository()
            => _exceptionHandle = new DbExceptionHandler();

        public Result ConnectStorage()
        {
            _dbConn = CreateDb();

            if (_dbConn != null)
                StorageConnection = CreateTableIfNot(_dbConn);

            return StorageConnection;
        }

        private IDbConnection CreateDb()
            => new OrmLiteConnectionFactory(
                "~/App_Data/todoDB.sqlite".MapHostAbsolutePath(),
                SqliteDialect.Provider)
                .Open();

        private Result CreateTableIfNot(IDbConnection dbConn)
        {
            try
            {
                dbConn.CreateTableIfNotExists<ItemDbModel>();
                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail($"Failed to create ItemDbModel table. Error: {ex.Message}");
            }
        }

        public Result Add(ItemDbModel item)
            => _exceptionHandle.SqlExceptionHandler(

                () => _dbConn.Exists<ItemDbModel>(new { Name = item.Name })
                ? Result.Fail("This item already exists.")
                : Result.Ok(_dbConn.Insert(item)),

                "Could not add item to DB.");

        public Result<IEnumerable<ItemDbModel>> List()
            => _exceptionHandle.SqlExceptionHandler(

                () => Result.Ok<IEnumerable<ItemDbModel>>(_dbConn.Select<ItemDbModel>()),

                "Failed to get DB table");

        public Result UpdateAllForce(IEnumerable<ItemDbModel> item_set)
            => _exceptionHandle.SqlExceptionHandler(() =>
            {
                var n = _dbConn.DeleteAll<ItemDbModel>();
                _dbConn.InsertAll(item_set);

                return Result.Ok(n);
            },
                "Failed to rewrite DB table");

        public Result UpdateItem(ItemDbModel item)
            => _exceptionHandle.SqlExceptionHandler(() =>
            {
                var dbItem = _dbConn.Single<ItemDbModel>((x) => x.Name == item.Name);

                if (dbItem == null)
                    return Result.Fail("Item not found");

                if (item.TimeStamp > dbItem.TimeStamp)
                    _dbConn.Update<ItemDbModel>(new { Name = item.Name, IsChecked = item.IsChecked });

;               return Result.Ok();
            },

                "Failed to update item");

        public Result DeleteByName(string name, DateTime timestamp)
            => _exceptionHandle.SqlExceptionHandler(() =>
            {
                var dbItem = _dbConn.Single<ItemDbModel>((x) => x.Name == name);

                if (dbItem == null)
                    return Result.Fail("Item not found");

                if (timestamp > dbItem.TimeStamp)
                    _dbConn.Delete(dbItem);

                return Result.Ok();
            },
                "Failed to delete item");

        public void Dispose()
        {
            _dbConn.Dispose();
            _dbConn = null;
        }
    }
}