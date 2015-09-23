/// A Signal which hands back an URL
/// 
/// string The URL
using strange.extensions.signal.impl;

namespace ctac.signals
{
    [Singleton]
    public class SocketConnectSignal : Signal<SocketKey> { }

    [Singleton]
    public class SocketMessageSignal : Signal<SocketKey> { }

    [Singleton]
    public class SocketErrorSignal : Signal<SocketKey, string> { }

    [Singleton]
    public class SocketDisconnectSignal : Signal<SocketKey> { }

    [Singleton]
    public class PingSignal : Signal<int, SocketKey> { }
}
