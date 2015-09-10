using System.Collections.Generic;
using CentauriTac.Messaging;
using Svelto.IoC;

namespace CentauriTac
{
    public class HandlerProvider : IHandlerProvider
    {
        private IContainer _container;

        public HandlerProvider(IContainer container)
        {
            _container = container;
        }

        public IHandler<T> GetCommandHandler<T>(T command) where T : ICommand
        {
            return _container.GetInstance<IHandler<T>>();
        }

        public IEnumerable<IHandler<T>> GetEventHandlers<T>(T @event) where T : IEvent
        {
            return _container.GetAllInstances<IHandler<T>>();
        }
    }
}