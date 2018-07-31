using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;
using Unity;
using Unity.Exceptions;

namespace ToDoList.Server
{
    public class UnityResolver : IDependencyResolver
    {
        private IUnityContainer _container;

        public UnityResolver(IUnityContainer container)
            => _container = container ?? throw new ArgumentNullException(nameof(container));

        public IDependencyScope BeginScope()
            => new UnityResolver(_container.CreateChildContainer());

        public void Dispose()
            => _container.Dispose();

        public object GetService(Type serviceType)
        {
            try
            {
                return _container.Resolve(serviceType);
            }
            catch(ResolutionFailedException)
            {
                return null;
            }
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            try
            {
                return _container.ResolveAll(serviceType);
            }
            catch (ResolutionFailedException)
            {
                return new object[0];
            }
        }
    }
}