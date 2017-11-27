using strange.extensions.signal.impl;

namespace ctac.signals
{
    [Singleton] public class MatchmakerQueueSignal : Signal<SocketKey> { }
    [Singleton] public class MatchmakerCurrentGameSignal : Signal<GameMetaModel, SocketKey> { }
    [Singleton] public class MatchmakerStatusSignal : Signal<MatchmakerStatusModel, SocketKey> { }
    [Singleton] public class MatchmakerDequeueSignal : Signal<SocketKey> { }
}

