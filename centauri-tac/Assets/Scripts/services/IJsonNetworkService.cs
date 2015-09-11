using System;

namespace ctac
{
    public interface IJsonNetworkService
    {
        void Request(string componentName, string methodName, Type type, object data = null);
        //Instead of an EventDispatcher, we put the actual Signals into the Interface
        FulfillWebServiceRequestSignal fulfillSignal { get; }
    }
}

