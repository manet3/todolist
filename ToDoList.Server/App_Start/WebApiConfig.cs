using AutoMapper;
using System.Web.Http;
using ToDoList.Server.Database.Models;
using ToDoList.Shared;
using ToDoList.Server.Database;
using Unity;
using Unity.Lifetime;

namespace ToDoList.Server
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            Mapper.Initialize(m => m.CreateMap<ToDoItem, ItemDbModel>());

            var container = new UnityContainer();
            container.RegisterType<IToDoItemsRepository, ToDoItemsLiteRepository>(new HierarchicalLifetimeManager());
            config.DependencyResolver = new UnityResolver(container);

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "ApiDefault",
                routeTemplate: "{action}",
                defaults: new { controller = "todo" });
        }
    }
}
