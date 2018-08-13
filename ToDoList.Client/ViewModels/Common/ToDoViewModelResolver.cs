using Unity;

namespace ToDoList.Client.ViewModels.Common
{
    class ToDoViewModelResolver
    {
        public ToDoViewModel ToDoViewModel
            => InjectionConfig.Container.Resolve<ToDoViewModel>();
    }
}
