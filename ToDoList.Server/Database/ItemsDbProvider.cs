using CSharpFunctionalExtensions;
using ServiceStack;
using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using ToDoList.Server.Database.Models;

namespace ToDoList.Server.Database
{
    public static class ItemsDbProvider
    {
        public static readonly string DbFilePath;

        private static readonly OrmLiteConnectionFactory _dbFactory;

        static ItemsDbProvider()
        {
            DbFilePath = "~/App_Data/todoDB.sqlite".MapHostAbsolutePath();
            _dbFactory = new OrmLiteConnectionFactory(DbFilePath, SqliteDialect.Provider);
        }

        public static Result CreateTableIfNotExists()
            => SqlExceptionHandler(
                dbConn => Result.Ok(dbConn.CreateTableIfNotExists<ItemDbModel>()),
                "Could not complete CreateTableIfNotExists.");

        public static Result AddToDB(ItemDbModel item)
            => SqlExceptionHandler(
                dbConn =>
                {
                    if (dbConn.Exists<ItemDbModel>(new { Name = item.Name }))
                        return Result.Fail("This item already exists.");
                    else return Result.Ok(dbConn.Insert(item));
                },
                "Could not add item to DB.");

        public static Result<IEnumerable<ItemDbModel>> GetDBItems()
            => SqlExceptionHandler(dbConn =>
                Result.Ok<IEnumerable<ItemDbModel>>(dbConn.Select<ItemDbModel>()),
                "Failed to get DB table");

        public static Result DBRewrite(IEnumerable<ItemDbModel> item_set)
            => SqlExceptionHandler(dbConn =>
            {
                var n = dbConn.DeleteAll<ItemDbModel>();
                dbConn.InsertAll(item_set);

                return Result.Ok(n);
            },
                "Failed to rewrite DB table");

        public static Result DBUpdateItem(ItemDbModel item)
            => SqlExceptionHandler(dbConn => Result.Ok((long)dbConn.Update(item)),
                "Failed to update item");

        public static Result TryRemoveDBItem(string name)
            => SqlExceptionHandler(dbConn =>
            {
                if (!dbConn.Exists<ItemDbModel>(new { Name = name }))
                    return Result.Fail("Item not found");
                return Result.Ok(dbConn.Delete<ItemDbModel>((x) => x.Name == name));
            }, 
                "Failed to delete item");

        private static Result<T> SqlExceptionHandler<T>(Func<IDbConnection, Result<T>> func, string customErrorMessagePart)
            => SqlExceptionHandlerCommon(func,
                ex => Result.Fail<T>($"{customErrorMessagePart} Error: {ex.Message}"),
                customErrorMessagePart);

        private static Result SqlExceptionHandler(Func<IDbConnection, Result> func, string customErrorMessagePart)
            => SqlExceptionHandlerCommon(func,
                ex => Result.Fail($"{customErrorMessagePart} Error: {ex.Message}"),
                customErrorMessagePart);

        private static T SqlExceptionHandlerCommon<T>(
            Func<IDbConnection, T> func, Func<SqlException, T> errorReturn,
            string customErrorMessagePart)
        {
            try
            {
                using (var dbConn = _dbFactory.Open())
                    return func.Invoke(dbConn);
            }
            catch (SqlException ex)
            {
                return errorReturn.Invoke(ex);
            }
        }
    }
}