using CentauriTac.Messaging;

namespace CentauriTac
{
    public interface IMessageDecorator
    {
        void Decorate(IMessage message);
    }
}