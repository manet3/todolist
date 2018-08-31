using CSharpFunctionalExtensions;
using System;
using System.Data.SqlClient;
using System.Data.SQLite;

namespace ToDoList.Server.Database.Services
{
    public class DbExceptionsHandler
    {
        public Result DbConfiguration = Result.Ok();

        public Result<TResult> DbExceptionsHandle<TResult>(Func<Result<TResult>> func, string customErrorMessagePart)
        {
            if (DbConfiguration.IsFailure)
                return Result.Fail<TResult>(DbConfiguration.Error);

            return SqlExceptionHandle(
                  func,
                  ex => Result.Fail<TResult>($"{customErrorMessagePart} Error: {ex.Message}"),
                  customErrorMessagePart);
        }

        public Result DbExceptionsHandle(Func<Result> func, string customErrorMessagePart)
        {
            if (DbConfiguration.IsFailure)
                return DbConfiguration;

            return SqlExceptionHandle(
                  func,
                  ex => Result.Fail($"{customErrorMessagePart} Error: {ex.Message}"),
                  customErrorMessagePart);
        }

        private static TResult SqlExceptionHandle<TResult>(
            Func<TResult> func,
            Func<Exception, TResult> errorReturn,
            string customErrorMessagePart)
        {
            try
            {
                return func.Invoke();
            }
            catch (Exception ex) when (ex is SqlException || ex is SQLiteException)
            {
                return errorReturn.Invoke(ex);
            }
        }
    }
}