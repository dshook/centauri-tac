using System.Collections.Generic;

namespace CentauriTac
{
    public interface IMessageDecoratorProvider
    {
        IEnumerable<IMessageDecorator> GetDecorators();
    }
}