/// A Signal which hands back an URL
/// 
/// string The URL
using strange.extensions.signal.impl;

namespace ctac.signals
{
    [Singleton]
    public class SocketConnectSignal : Signal<string> { }

    [Singleton]
    public class SocketMessageSignal : Signal<string> { }

    [Singleton]
    public class SocketErrorSignal : Signal<string, string> { }

    [Singleton]
    public class SocketDisconnectSignal : Signal<string> { }
}

