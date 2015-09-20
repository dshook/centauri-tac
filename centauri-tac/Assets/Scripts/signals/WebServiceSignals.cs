using strange.extensions.signal.impl;

namespace ctac.signals
{
    public class CallWebServiceSignal : Signal<string> { }
    public class FulfillWebServiceRequestSignal : Signal<string, object> { }
}

