using AutoMapper;
using System.Web.Http;
using ToDoList.Server.Database.POCOs;
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
            var container = new UnityContainer();
            container.RegisterType<IStorageRepository, ItemsRepository>(new HierarchicalLifetimeManager());
            container.RegisterType<IStorageRepository, ItemsRepository>(new HierarchicalLifetimeManager());

            config.DependencyResolver = new UnityResolver(container);

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "ApiDefault",
                routeTemplate: "{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional});
        }
    }
}
