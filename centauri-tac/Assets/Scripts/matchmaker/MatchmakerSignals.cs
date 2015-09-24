using strange.extensions.signal.impl;

namespace ctac.signals
{
    [Singleton]
    public class MatchmakerLoggedInSignal : Signal<LoginStatusModel, SocketKey> { }

    [Singleton]
    public class MatchmakerCurrentGameSignal : Signal<GameModel, SocketKey> { }

    [Singleton]
    public class MatchmakerStatusSignal : Signal<MatchmakerStatusModel, SocketKey> { }
}

