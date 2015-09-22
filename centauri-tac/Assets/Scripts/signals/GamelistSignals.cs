using strange.extensions.signal.impl;

namespace ctac.signals
{
    [Singleton]
    public class GamelistGameSignal : Signal<GamelistGameModel, SocketKey> { }

    [Singleton]
    public class GamelistCurrentGameSignal : Signal<GamelistGameModel, SocketKey> { }
}

