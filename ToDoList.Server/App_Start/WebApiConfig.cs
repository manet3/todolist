using AutoMapper;
using System.Web.Http;
using ToDoList.Server.Database.Models;
using ToDoList.Shared;
using ToDoList.Server.Database;
using System;

namespace ToDoList.Server
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            Mapper.Initialize(m => m.CreateMap<ToDoItem, ItemDbModel>());

            ItemsDbProvider.CreateTableIfNotExists();

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "ApiDefault",
                routeTemplate: "{action}",
                defaults: new { controller = "todo" });
        }
    }
}
