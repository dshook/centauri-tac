using strange.extensions.signal.impl;

namespace ctac.signals
{
    [Singleton]
    public class AuthMatchmakerSignal : Signal<PlayerModel, SocketKey> { }

    [Singleton]
    public class MatchmakerLoggedInSignal : Signal<LoginStatusModel, SocketKey> { }

    [Singleton]
    public class MatchmakerCurrentGameSignal : Signal<GameMetaModel, SocketKey> { }

    [Singleton]
    public class MatchmakerStatusSignal : Signal<MatchmakerStatusModel, SocketKey> { }
}

