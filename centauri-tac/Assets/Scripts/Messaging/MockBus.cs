using System.Collections.Generic;

namespace CentauriTac
{
    public class MockBus : IBus
    {
        public readonly List<IEvent> Events = new List<IEvent>();
        public readonly List<ICommand> Commands = new List<ICommand>();

        public void Send(params ICommand[] commands)
        {
            Commands.AddRange(commands);
        }

        public void Publish(params IEvent[] events)
        {
            Events.AddRange(events);
        }
    }
}