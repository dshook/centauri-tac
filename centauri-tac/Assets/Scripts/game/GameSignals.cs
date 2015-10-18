using strange.extensions.signal.impl;

namespace ctac.signals
{
    [Singleton]
    public class CurrentGameSignal : Signal<GameMetaModel, SocketKey> { }

    [Singleton]
    public class GameLoggedInSignal : Signal<LoginStatusModel, SocketKey> { }

    [Singleton]
    public class PlayerConnectSignal : Signal<GameJoinConnectModel, SocketKey> { }

    [Singleton]
    public class PlayerJoinedSignal : Signal<GameJoinConnectModel, SocketKey> { }

    [Singleton]
    public class PlayerPartSignal : Signal<GameJoinConnectModel, SocketKey> { }

    [Singleton]
    public class PlayerDisconnectSignal : Signal<GameJoinConnectModel, SocketKey> { }

    [Singleton]
    public class LeaveGameSignal : Signal<SocketKey> { }

    [Singleton]
    public class TileHoverSignal : Signal<Tile> { }

    [Singleton]
    public class MapCreatedSignal : Signal { }

    [Singleton]
    public class PieceSelectedSignal : Signal<PieceModel> { }

    [Singleton]
    public class MovePieceSignal : Signal<PieceModel, Tile> { }
    [Singleton]
    public class ActionPieceMovedSignal : Signal<MovePieceModel, SocketKey> { }
    [Singleton]
    public class PieceMovedSignal : Signal<PieceModel, Tile> { }

    [Singleton]
    public class AttackPieceSignal : Signal<AttackPieceModel> { }
    [Singleton]
    public class ActionPieceAttackedSignal : Signal<AttackPieceModel, SocketKey> { }
    [Singleton]
    public class PieceAttackedSignal : Signal<AttackPieceModel> { }
    [Singleton]
    public class PieceAttackedAnimationSignal : Signal<PieceModel> { }

    [Singleton]
    public class ServerQueueProcessStart : Signal<int> { }

    [Singleton]
    public class ServerQueueProcessEnd : Signal<int> { }

    [Singleton]
    public class EndTurnSignal : Signal { }
    [Singleton]
    public class ActionEndTurnSignal : Signal<GamePassTurnModel> { }
    [Singleton]
    public class TurnEndedSignal : Signal { }

    [Singleton]
    public class ActionSpawnPieceSignal : Signal<SpawnPieceModel, SocketKey> { }

    [Singleton]
    public class PieceDiedSignal : Signal<PieceModel> { }

    [Singleton]
    public class CardSelectedSignal : Signal<CardModel> { }

}

