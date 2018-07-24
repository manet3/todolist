using CSharpFunctionalExtensions;
using ServiceStack;
using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using ToDoList.Server.Database.Models;

namespace ToDoList.Server.Database
{
    public class ToDoItemsLiteRepository : IToDoItemsRepository, IDisposable
    {
        private IDbConnection dbConn;

        private DbExceptionHandle<SqlException> exceptionHandle;

        public Result TableCreation { get; private set; }

        public ToDoItemsLiteRepository()
        {
            exceptionHandle = new DbExceptionHandle<SqlException>();
            DbInit();
        }

        private void DbInit()
        {
            //open db connection
            dbConn = (new OrmLiteConnectionFactory(
                "~/App_Data/todoDB.sqlite".MapHostAbsolutePath(),
                SqliteDialect.Provider))
                .Open();

            //create db table
            try
            {
                dbConn.CreateTableIfNotExists<ItemDbModel>();
                TableCreation = Result.Ok();
            }
            catch (Exception ex)
            {
                TableCreation = Result.Fail($"Failed to create ItemDbModel table. Error: {ex.Message}");
            }
        }

        public Result Add(ItemDbModel item)
            => exceptionHandle.SqlExceptionHandler(

                () => dbConn.Exists<ItemDbModel>(new { Name = item.Name })
                ? Result.Fail("This item already exists.")
                :Result.Ok(dbConn.Insert(item)),

                "Could not add item to DB.");

        public Result<IEnumerable<ItemDbModel>> List()
            => exceptionHandle.SqlExceptionHandler(

                () => Result.Ok<IEnumerable<ItemDbModel>>(dbConn.Select<ItemDbModel>()),

                "Failed to get DB table");

        public Result UpdateAll(IEnumerable<ItemDbModel> item_set)
            => exceptionHandle.SqlExceptionHandler(() =>
            {
                var n = dbConn.DeleteAll<ItemDbModel>();
                dbConn.InsertAll(item_set);

                return Result.Ok(n);
            },
                "Failed to rewrite DB table");

        public Result UpdateItem(ItemDbModel item)
            => exceptionHandle.SqlExceptionHandler(

                () => !dbConn.Exists<ItemDbModel>(new { Name = item.Name })
                ? Result.Fail("Item not found")
                : Result.Ok(dbConn.Update(item)),

                "Failed to update item");

        public Result DeleteByName(string name)
            => exceptionHandle.SqlExceptionHandler(

                () => !dbConn.Exists<ItemDbModel>(new { Name = name })
                ? Result.Fail("Item not found")
                : Result.Ok(dbConn.Delete<ItemDbModel>((x) => x.Name == name)),

                "Failed to delete item");

        public void Dispose()
        {
            dbConn.Dispose();
            dbConn = null;
        }
    }
}