using CSharpFunctionalExtensions;
using System;

namespace ToDoList.Server.Database
{
    public class DbExceptionHandle<TdbException>
        where TdbException : Exception
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
            Func<TdbException, TResult> errorReturn,
            string customErrorMessagePart)
        {
            try
            {
                return func.Invoke();
            }
            catch (TdbException ex)
            {
                return errorReturn.Invoke(ex);
            }
        }
    }
}