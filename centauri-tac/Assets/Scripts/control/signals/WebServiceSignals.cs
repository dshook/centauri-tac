using strange.extensions.signal.impl;

namespace ctac.signals
{
    [Singleton]
    public class CallWebServiceSignal : Signal<string> { }

    [Singleton]
    public class FulfillWebServiceRequestSignal : Signal<string, object> { }
}

