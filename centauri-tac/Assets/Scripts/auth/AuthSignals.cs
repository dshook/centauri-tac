using strange.extensions.signal.impl;

namespace ctac.signals
{
    [Singleton]
    public class ServerAuthSignal : Signal { }

    [Singleton]
    public class TryLoginSignal : Signal<Credentials> { }

    [Singleton]
    public class FailedAuthSignal : Signal { }

    [Singleton]
    public class TokenSignal : Signal<string, SocketKey> { }

    [Singleton]
    public class TryRegisterSignal : Signal<Credentials> { }

    [Singleton]
    [ManualMapSignal]
    public class PlayerFetchedSignal : Signal<PlayerModel, SocketKey> { }

    [Singleton]
    public class PlayerFetchedFinishedSignal : Signal<PlayerModel, SocketKey> { }

    [Singleton]
    public class NeedLoginSignal : Signal<string> { }

    [Singleton]
    public class ComponentLoggedInSignal : Signal<LoginStatusModel, SocketKey> { }

    [Singleton]
    public class RegisteredSignal : Signal<LoginStatusModel, SocketKey> { }

    [Singleton]
    public class AuthLoggedInSignal : Signal<LoginStatusModel, SocketKey> { }

    [Singleton]
    public class AuthLogoutSignal : Signal { }

    [Singleton]
    public class LatencySignal : Signal<decimal> { }

}

