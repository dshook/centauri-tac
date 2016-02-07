using strange.extensions.signal.impl;

namespace ctac.signals
{
    [Singleton]
    public class CurrentGameSignal : Signal<GameMetaModel, SocketKey> { }

    [Singleton]
    public class GameLoggedInSignal : Signal<LoginStatusModel, SocketKey> { }
    [Singleton]
    public class GameFinishedSignal : Signal<GameFinishedModel, SocketKey> { }

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
    public class PossibleActionsSignal : Signal<PossibleActions, SocketKey> { }

    [Singleton]
    public class TileHoverSignal : Signal<Tile> { }

    [Singleton]
    public class MapCreatedSignal : Signal { }

    [Singleton]
    public class PieceSelectedSignal : Signal<PieceModel> { }

    [Singleton]
    public class MovePieceSignal : Signal<PieceModel, Tile> { }
    [Singleton]
    public class ActionMovePieceSignal : Signal<MovePieceModel, SocketKey> { }
    [Singleton]
    public class PieceMovedSignal : Signal<PieceMovedModel> { }
    [Singleton]
    public class PieceFinishedMovingSignal : Signal<PieceModel> { }

    [Singleton]
    public class AttackPieceSignal : Signal<AttackPieceModel> { }
    [Singleton]
    public class ActionAttackPieceSignal : Signal<AttackPieceModel, SocketKey> { }
    [Singleton]
    public class PieceAttackedSignal : Signal<AttackPieceModel> { }
    [Singleton]
    public class PieceTextAnimationFinishedSignal : Signal<PieceModel> { }

    [Singleton]
    public class ActionPieceHealthChangeSignal : Signal<PieceHealthChangeModel, SocketKey> { }
    [Singleton]
    public class PieceHealthChangedSignal : Signal<PieceHealthChangeModel> { }
    [Singleton]
    public class ActionPieceAttributeChangeSignal : Signal<PieceAttributeChangeModel, SocketKey> { }
    [Singleton]
    public class PieceAttributeChangedSignal : Signal<PieceAttributeChangeModel> { }
    [Singleton]
    public class ActionPieceBuffSignal : Signal<PieceBuffModel, SocketKey> { }
    [Singleton]
    public class PieceBuffSignal : Signal<PieceBuffModel> { }

    [Singleton]
    public class ActionPlaySpellSignal : Signal<PlaySpellModel> { }

    [Singleton]
    public class PieceHoverSignal : Signal<PieceModel> { }

    [Singleton]
    public class ServerQueueProcessStart : Signal<int> { }
    [Singleton]
    public class ServerQueueProcessEnd : Signal<int> { }

    [Singleton]
    public class EndTurnSignal : Signal { }
    [Singleton]
    public class ActionPassTurnSignal : Signal<PassTurnModel> { }
    [Singleton]
    public class TurnEndedSignal : Signal { }

    [Singleton]
    public class ActionSpawnPieceSignal : Signal<SpawnPieceModel, SocketKey> { }
    [Singleton]
    public class PieceSpawnedSignal : Signal<PieceModel> { }
    [Singleton]
    public class ActionSpawnPieceCancelledSignal : Signal<SpawnPieceModel, SocketKey> { }

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
    public class ActivateCardSignal : Signal<ActivateModel> { }
    [Singleton]
    public class ActionActivateCardSignal : Signal<ActivateCardModel> { }
    [Singleton]
    public class DestroyCardSignal : Signal<int> { }
    [Singleton]
    public class CardDestroyedSignal : Signal<CardModel> { }

    [Singleton]
    public class StartSelectTargetSignal : Signal<CardModel, Tile, ActionTarget> { }
    [Singleton]
    public class SelectTargetSignal : Signal<CardModel, PieceModel> { }
    [Singleton]
    public class CancelSelectTargetSignal : Signal<CardModel> { }

    [Singleton]
    public class ActionSetPlayerResourceSignal : Signal<SetPlayerResourceModel> { }
    [Singleton]
    public class PlayerResourceSetSignal : Signal<SetPlayerResourceModel> { }

}

