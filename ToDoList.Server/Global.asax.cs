using System;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using ToDoList.Server.Database;

namespace ToDoList.Server
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            ItemsDbProvider.CreateDbTable();

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
    }
}
