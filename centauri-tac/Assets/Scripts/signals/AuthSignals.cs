using strange.extensions.signal.impl;
using ctac;

namespace ctac.signals
{
    public class ComponentsFetchedSignal : Signal { }
    public class FetchComponentsSignal : Signal { }

    [Singleton]
    public class TryLoginSignal : Signal<string, string> { }

    [Singleton]
    public class FailedAuthSignal : Signal { }

    [Singleton]
    public class TokenSignal : Signal<string, SocketKey> { }

    [Singleton]
    public class PlayerFetchedSignal : Signal<PlayerModel, SocketKey> { }

    [Singleton]
    public class NeedLoginSignal : Signal { }

    [Singleton]
    public class LoggedInSignal : Signal<LoginStatusModel, SocketKey> { }

    [Singleton]
    public class LatencySignal : Signal<decimal> { }


}

