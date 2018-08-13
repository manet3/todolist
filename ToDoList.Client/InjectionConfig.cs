using ToDoList.Client.DataServices;
using Unity;

namespace ToDoList.Client
{
    static class InjectionConfig
    {
        public static IUnityContainer Container { get; }
            = new UnityContainer();

        public static void RegisterDataServices()
        {
            Container.RegisterType<ISync, Sync>();
            Container.RegisterType<IRequestSender, RequestSender>();
        }
    }
}
