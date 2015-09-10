using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CentauriTac.Messaging;

namespace CentauriTac
{
    public class Bus : IBus
    {
        private IHandlerProvider _handlerProvider;
        private IMessageDecoratorProvider _decoratorProvider;

        public Bus(IHandlerProvider handlerProvider, IMessageDecoratorProvider decoratorProvider)
        {
            _handlerProvider = handlerProvider;
            _decoratorProvider = decoratorProvider;
        }

        public void Send(IEnumerable<ICommand> commands)
        {
            Send((commands ?? Enumerable.Empty<ICommand>()).ToArray());
        }

        public void Send(params ICommand[] commands)
        {
            if (commands == null || !commands.Any())
                return;

            var sendInternal = typeof (Bus)
                .GetMethod("SendInternal", BindingFlags.Instance | BindingFlags.NonPublic);

            foreach (var command in commands) {
                RunDecorators(command);

                try {
                    sendInternal
                        .MakeGenericMethod(new[] {command.GetType()})
                        .Invoke(this, new[] {command});
                }
                catch (TargetInvocationException ex) {
                    throw ex.InnerException;
                }
            }
        }

        public void Publish(IEnumerable<IEvent> commands)
        {
            Publish((commands ?? Enumerable.Empty<IEvent>()).ToArray());
        }

        public void Publish(params IEvent[] events)
        {
            if (events == null || !events.Any())
                return;

            var publishInternal = 
                typeof (Bus)
                    .GetMethod("PublishInternal", BindingFlags.Instance | BindingFlags.NonPublic);

            foreach (var @event in events)
            {
                RunDecorators(@event);

                try
                {
                    publishInternal
                        .MakeGenericMethod(new[] { @event.GetType() })
                        .Invoke(this, new[] { @event });
                }
                catch (TargetInvocationException ex)
                {
                    throw ex.InnerException;
                }
            }
        }

        private void RunDecorators(IMessage message)
        {
            foreach (var decorator in _decoratorProvider.GetDecorators()) {
                decorator.Decorate(message);
            }
        }

        private void SendInternal<TCommand>(TCommand command) where TCommand : class, ICommand
        {
            var handler = _handlerProvider.GetCommandHandler(command);
            if (handler == null)
                throw new Exception(string.Format("There is no handler registered for the command of type '{0}'.",command.GetType()));

            handler.Handle(command);
        }

        private void PublishInternal<TEvent>(TEvent @event) where TEvent : class, IEvent
        {
            var handlers = _handlerProvider.GetEventHandlers(@event);
            foreach (var handler in handlers) {
                handler.Handle(@event);
            }
        }
    }
}