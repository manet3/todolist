using System.Net.Http;

namespace ToDoList.Client.DataServices
{
    public class ApiAction
    {
        public HttpMethod Method;
        public string Name;

        public ApiAction(HttpMethod method, string name)
            => (Method, Name) = (method, name);

        public static ApiAction List = new ApiAction(HttpMethod.Get, "list");

        public static ApiAction Change = new ApiAction(HttpMethod.Put, "change");

        public static ApiAction Add = new ApiAction(HttpMethod.Post, "add");

        public static ApiAction Delete = new ApiAction(HttpMethod.Delete, "delete");

        public override bool Equals(object obj)
            => obj is ApiAction action && action.Name.Equals(Name);

        public override int GetHashCode()
            => Name.GetHashCode();
    }
}
