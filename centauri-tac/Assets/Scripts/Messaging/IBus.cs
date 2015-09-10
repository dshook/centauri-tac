namespace CentauriTac
{
    public interface IBus
    {
        void Send(params ICommand[] commands);
        void Publish(params IEvent[] events);
    }
}