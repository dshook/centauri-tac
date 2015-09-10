using System.Collections.Generic;
using Svelto.IoC;

namespace CentauriTac
{
    public class DecoratorProvider : IMessageDecoratorProvider
    {
        private IContainer _container;

        public DecoratorProvider(IContainer container)
        {
            _container = container;
        }

        public IEnumerable<IMessageDecorator> GetDecorators()
        {
            return _container.GetAllInstances<IMessageDecorator>();
        }
    }
}