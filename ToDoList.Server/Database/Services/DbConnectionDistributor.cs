using CSharpFunctionalExtensions;
using ServiceStack;
using ServiceStack.OrmLite;
using System.Data;

namespace ToDoList.Server.Database.Services
{
    public static class DbConnectionDistributor
    {
        public static Result DbConnectingResult => _dbConnection == null ? Result.Fail("No DB connection.") : Result.Ok();

        private static IDbConnection _dbConnection;
        public static IDbConnection DbConnection
        {
            get
            {
                if (_dbConnection == null)
                    return new OrmLiteConnectionFactory(
                       "~/App_Data/todoDB.sqlite".MapHostAbsolutePath(),
                       SqliteDialect.Provider)
                       .Open();

                return _dbConnection;
            }
        }
    }
}