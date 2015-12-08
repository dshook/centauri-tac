using strange.extensions.signal.impl;

namespace ctac.signals
{
    [Singleton]
    public class CurrentGameSignal : Signal<GameMetaModel, SocketKey> { }

    [Singleton]
    public class GameLoggedInSignal : Signal<LoginStatusModel, SocketKey> { }

    [Singleton]
    public class PlayerConnectSignal : Signal<JoinOrConnectModel, SocketKey> { }

    [Singleton]
    public class PlayerJoinedSignal : Signal<JoinOrConnectModel, SocketKey> { }

    [Singleton]
    public class PlayerPartSignal : Signal<JoinOrConnectModel, SocketKey> { }

    [Singleton]
    public class PlayerDisconnectSignal : Signal<JoinOrConnectModel, SocketKey> { }

    [Singleton]
    public class LeaveGameSignal : Signal<SocketKey> { }

    [Singleton]
    public class ActionMessageSignal : Signal<MessageModel, SocketKey> { }

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
    public class PieceMovedSignal : Signal<PieceMovedModel> { }
    [Singleton]
    public class PieceFinishedMovingSignal : Signal<PieceModel> { }

    [Singleton]
    public class AttackPieceSignal : Signal<AttackPieceModel> { }
    [Singleton]
    public class ActionPieceAttackedSignal : Signal<AttackPieceModel, SocketKey> { }
    [Singleton]
    public class PieceAttackedSignal : Signal<AttackPieceModel> { }
    [Singleton]
    public class PieceAttackedAnimationSignal : Signal<PieceModel> { }

    [Singleton]
    public class ActionPieceHealthChangedSignal : Signal<PieceHealthChangeModel, SocketKey> { }
    [Singleton]
    public class PieceHealthChangedSignal : Signal<PieceHealthChangeModel> { }
    [Singleton]
    public class ActionPieceAttributeChangedSignal : Signal<PieceAttributeChangeModel, SocketKey> { }
    [Singleton]
    public class PieceAttributeChangedSignal : Signal<PieceAttributeChangeModel> { }

    [Singleton]
    public class PieceHoverSignal : Signal<PieceModel> { }

    [Singleton]
    public class ServerQueueProcessStart : Signal<int> { }
    [Singleton]
    public class ServerQueueProcessEnd : Signal<int> { }

    [Singleton]
    public class EndTurnSignal : Signal { }
    [Singleton]
    public class ActionEndTurnSignal : Signal<PassTurnModel> { }
    [Singleton]
    public class TurnEndedSignal : Signal { }

    [Singleton]
    public class ActionSpawnPieceSignal : Signal<SpawnPieceModel, SocketKey> { }
    [Singleton]
    public class PieceSpawnedSignal : Signal<PieceModel> { }

    [Singleton]
    public class PieceDiedSignal : Signal<PieceModel> { }

    [Singleton]
    public class ActionDrawCardSignal : Signal<DrawCardModel, SocketKey> { }
    [Singleton]
    public class CardDrawnSignal : Signal<CardModel> { }
    [Singleton]
    public class CardDrawShownSignal : Signal<CardModel> { }

    [Singleton]
    public class ActionSpawnDeckSignal : Signal<SpawnDeckModel, SocketKey> { }
    [Singleton]
    public class DeckSpawnedSignal : Signal<SpawnDeckModel> { }

    [Singleton]
    public class CardSelectedSignal : Signal<CardModel> { }
    [Singleton]
    public class CardHoveredSignal : Signal<CardModel> { }
    [Singleton]
    public class ActivateCardSignal : Signal<CardModel, Tile> { }
    [Singleton]
    public class ActionActivateCardSignal : Signal<ActivateCardModel> { }
    [Singleton]
    public class DestroyCardSignal : Signal<int> { }
    [Singleton]
    public class CardDestroyedSignal : Signal<CardModel> { }

    [Singleton]
    public class ActionSetPlayerResourceSignal : Signal<SetPlayerResourceModel> { }
    [Singleton]
    public class PlayerResourceSetSignal : Signal<SetPlayerResourceModel> { }

}

