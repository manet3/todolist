using CSharpFunctionalExtensions;
using System;
using System.Data.SqlClient;

namespace ToDoList.Server.Database
{
    public class DbExceptionHandler
    {
        public Result<TResult> SqlExceptionHandler<TResult>(Func<Result<TResult>> func, string customErrorMessagePart)
            => SqlExceptionHandlerCommon(
                func,
                ex => Result.Fail<TResult>($"{customErrorMessagePart} Error: {ex.Message}"),
                customErrorMessagePart);

        public Result SqlExceptionHandler(Func<Result> func, string customErrorMessagePart)
            => SqlExceptionHandlerCommon(
                func,
                ex => Result.Fail($"{customErrorMessagePart} Error: {ex.Message}"),
                customErrorMessagePart);

        private TResult SqlExceptionHandlerCommon<TResult>(
            Func<TResult> func,
            Func<SqlException, TResult> errorReturn,
            string customErrorMessagePart)
        {
            try
            {
                return func.Invoke();
            }
            catch (SqlException ex)
            {
                return errorReturn.Invoke(ex);
            }
        }
    }
}