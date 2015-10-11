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
    public class MinionSelectedSignal : Signal<MinionModel> { }

    [Singleton]
    public class MoveMinionSignal : Signal<MinionModel, Tile> { }
    [Singleton]
    public class ActionMovePieceSignal : Signal<MovePieceModel, SocketKey> { }
    [Singleton]
    public class MinionMovedSignal : Signal<MinionModel, Tile> { }

    [Singleton]
    public class AttackMinionSignal : Signal<AttackPieceModel> { }
    [Singleton]
    public class ActionAttackPieceSignal : Signal<AttackPieceModel, SocketKey> { }
    [Singleton]
    public class MinionAttackedSignal : Signal<AttackPieceModel> { }
    [Singleton]
    public class MinionAttackedAnimationSignal : Signal<MinionModel> { }

    [Singleton]
    public class ServerQueueProcessStart : Signal<int> { }

    [Singleton]
    public class ServerQueueProcessEnd : Signal<int> { }

    [Singleton]
    public class EndTurnSignal : Signal { }
    [Singleton]
    public class ActionPassTurnSignal : Signal<GamePassTurnModel> { }
    [Singleton]
    public class TurnEndedSignal : Signal { }

    [Singleton]
    public class ActionSpawnPieceSignal : Signal<SpawnPieceModel, SocketKey> { }

    [Singleton]
    public class PieceDiedSignal : Signal<MinionModel> { }

}

