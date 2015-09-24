using strange.extensions.signal.impl;

namespace ctac.signals
{
    [Singleton]
    public class GamelistLoggedInSignal : Signal<LoginStatusModel, SocketKey> { }

    [Singleton]
    public class GamelistGameSignal : Signal<GameModel, SocketKey> { }
}

