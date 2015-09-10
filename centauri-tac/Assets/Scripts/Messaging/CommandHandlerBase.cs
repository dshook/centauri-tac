using System.Collections.Generic;
using CentauriTac.Messaging;

namespace CentauriTac
{
    public abstract class CommandHandlerBase<T> : IHandler<T> where T : ICommand
    {
        private IBus _bus;
        private readonly List<IEvent> _events = new List<IEvent>();

        protected CommandHandlerBase(IBus bus)
        {
            _bus = bus;
        }

        protected void EnqueueEvent(IEvent @event)
        {
            _events.Add(@event);
        }

        public void Handle(T args)
        {
            HandleInternal(args);
            _bus.Publish(_events.ToArray());
        }

        protected abstract void HandleInternal(T args);
    }
}