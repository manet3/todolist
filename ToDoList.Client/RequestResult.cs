namespace ToDoList.Client
{
    public enum RequestErrorType
    {
        None,
        Cancelled,
        NoConnection,
        ServerError
    }

    public struct RequestError
    {
        public readonly string Message;

        public readonly RequestErrorType Type;

        public RequestError(string message, RequestErrorType errorType)
            => (Message, Type) = (message, errorType);
    }

    public struct RequestResult
    {
        public bool IsFailure;
        public RequestError Error;

        public static RequestResult Ok() => default;

        public static RequestResult Fail(RequestError error)
            => new RequestResult { IsFailure = true, Error = error };

        public static RequestResult<T> Ok<T>(T value)
            => new RequestResult<T> { Value = value };

        public static RequestResult<T> Fail<T>(RequestError error)
            => new RequestResult<T> { IsFailure = true, Error = error };
    }

    public struct RequestResult<T>
    {
        public bool IsFailure;
        public RequestError Error;
        public T Value;

        public static implicit operator RequestResult(RequestResult<T> valueResult)
            => new RequestResult { IsFailure = valueResult.IsFailure, Error = valueResult.Error };
    }
}
