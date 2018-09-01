using CSharpFunctionalExtensions;
using ServiceStack;
using ServiceStack.OrmLite;
using System.Data;

namespace ToDoList.Server.Database.Services
{
    public static class DbConnectionDistributor
    {
        public static Result DbConnectingResult => DbConnection == null ? Result.Fail("No DB connection.") : Result.Ok();

        public static IDbConnection DbConnection { get; private set; }

        public static IDbConnection OpenConnection()
            => DbConnection = new OrmLiteConnectionFactory(
                       "~/App_Data/todoDB.sqlite".MapHostAbsolutePath(),
                       SqliteDialect.Provider)
                       .Open();

        public static void CloseConnection()
        {
            DbConnection?.Dispose();
            DbConnection = null;
        }
    }
}