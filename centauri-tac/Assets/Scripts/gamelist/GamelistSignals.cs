using strange.extensions.signal.impl;

namespace ctac.signals
{
    [Singleton]
    public class GamelistLoggedInSignal : Signal<LoginStatusModel, SocketKey> { }

    [Singleton]
    public class GamelistGameSignal : Signal<GamelistGameModel, SocketKey> { }

    [Singleton]
    public class GamelistCurrentGameSignal : Signal<GamelistGameModel, SocketKey> { }

    [Singleton]
    public class GameLoggedInSignal : Signal<LoginStatusModel, SocketKey> { }
}

