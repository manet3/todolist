using CSharpFunctionalExtensions;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ToDoList.Server.Database;

namespace ToDoList.Server.Controllers
{
    public static class ControllerExceptionHandler
    {
        public static void ThrowIfNullArgument(object argument)
        {
            if (argument == null)
                throw GetExceptionWith("Failed to get client data.", HttpStatusCode.BadRequest);
        }

        public static void ThrowIfFailure(Result result)
        {
            if (result.IsFailure)
                throw GetExceptionWith(result.Error, HttpStatusCode.InternalServerError);
        }

        public static HttpResponseException GetExceptionWith(string reasonForFailure, HttpStatusCode statusCode)
            => new HttpResponseException(new HttpResponseMessage(statusCode)
            { ReasonPhrase = reasonForFailure.Replace("\r\n", " | ") });

    }
}