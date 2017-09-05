using strange.extensions.signal.impl;

namespace ctac.signals
{
    [Singleton] public class CurrentGameSignal : Signal<GameMetaModel, SocketKey> { }

    [Singleton] public class GameLoggedInSignal : Signal<LoginStatusModel, SocketKey> { }
    [Singleton] public class GameAuthedSignal : Signal { }
    [Singleton] public class JoinGameSignal : Signal<LoginStatusModel, SocketKey> { }

    [Singleton] public class ActionGameFinishedSignal : Signal<GameFinishedModel, SocketKey> { }
    [Singleton] public class GameFinishedSignal : Signal<GameFinishedModel> { }

    [Singleton] public class PlayerConnectSignal : Signal<JoinOrConnectModel, SocketKey> { }
    [Singleton] public class PlayerJoinedSignal : Signal<JoinOrConnectModel, SocketKey> { }
    [Singleton] public class PlayerPartSignal : Signal<JoinOrConnectModel, SocketKey> { }
    [Singleton] public class PlayerDisconnectSignal : Signal<JoinOrConnectModel, SocketKey> { }

    [Singleton] public class LeaveGameSignal : Signal<SocketKey, bool> { }

    [Singleton] public class ActionMessageSignal : Signal<MessageModel, SocketKey> { }
    [Singleton] public class MessageSignal : Signal<MessageModel> { }

    [Singleton] public class ActionKickoffSignal : Signal<KickoffModel, SocketKey> { }

    [Singleton] public class PossibleActionsSignal : Signal<PossibleActions, SocketKey> { }
    [Singleton] public class PossibleActionsReceivedSignal : Signal<PossibleActions> { }

    [Singleton] public class TileHoverSignal : Signal<Tile> { }

    [Singleton] public class MapCreatedSignal : Signal { }

    [Singleton] public class PieceSelectedSignal : Signal<PieceModel> { }

    [Singleton] public class MovePieceSignal : Signal<PieceModel, Tile> { }
    [Singleton] public class ActionMovePieceSignal : Signal<MovePieceModel, SocketKey> { }
    [Singleton] public class PieceMovedSignal : Signal<PieceMovedModel> { }
    [Singleton] public class PieceFinishedMovingSignal : Signal<PieceModel> { }

    [Singleton] public class RotatePieceSignal : Signal<RotatePieceModel> { }
    [Singleton] public class ActionRotatePieceSignal : Signal<RotatePieceModel, SocketKey> { }
    [Singleton] public class PieceRotatedSignal : Signal<RotatePieceModel> { }

    [Singleton] public class AttackPieceSignal : Signal<AttackPieceModel> { }
    [Singleton] public class ActionAttackPieceSignal : Signal<AttackPieceModel, SocketKey> { }
    [Singleton] public class PieceAttackedSignal : Signal<AttackPieceModel> { }
    [Singleton] public class PieceTextAnimationFinishedSignal : Signal<PieceModel> { }

    [Singleton] public class ActionPieceStatusChangeSignal : Signal<PieceStatusChangeModel, SocketKey> { }
    [Singleton] public class PieceStatusChangeSignal : Signal<PieceStatusChangeModel> { }

    [Singleton] public class ActionPieceHealthChangeSignal : Signal<PieceHealthChangeModel, SocketKey> { }
    [Singleton] public class PieceHealthChangedSignal : Signal<PieceHealthChangeModel> { }
    [Singleton] public class ActionPieceAttributeChangeSignal : Signal<PieceAttributeChangeModel, SocketKey> { }
    [Singleton] public class PieceAttributeChangedSignal : Signal<PieceAttributeChangeModel> { }
    [Singleton] public class ActionPieceBuffSignal : Signal<PieceBuffModel, SocketKey> { }
    [Singleton] public class PieceBuffSignal : Signal<PieceBuffModel> { }

    [Singleton] public class ActionPieceAuraSignal : Signal<PieceAuraModel, SocketKey> { }
    [Singleton] public class PieceAuraSignal : Signal<PieceAuraModel> { }

    [Singleton] public class ActionCardBuffSignal : Signal<CardBuffModel, SocketKey> { }
    [Singleton] public class CardBuffSignal : Signal<CardBuffModel> { }

    [Singleton] public class ActionPieceArmorChangeSignal : Signal<PieceArmorChangeModel, SocketKey> { }
    [Singleton] public class PieceArmorChangedSignal : Signal<PieceArmorChangeModel> { }

    [Singleton] public class ActionPlaySpellSignal : Signal<PlaySpellModel> { }
    [Singleton] public class SpellPlayedSignal : Signal<SpellPlayedModel> { }
    [Singleton] public class ActionCancelledPlaySpellSignal : Signal<PlaySpellModel, SocketKey> { }

    [Singleton] public class PieceHoverSignal : Signal<PieceModel> { }

    [Singleton] public class ServerQueueProcessStart : Signal<int> { }
    [Singleton] public class ServerQueueProcessEnd : Signal<int> { }

    [Singleton] public class EndTurnSignal : Signal { }
    [Singleton] public class ActionPassTurnSignal : Signal<PassTurnModel> { }
    [Singleton] public class TurnEndedSignal : Signal<GameTurnModel> { }

    [Singleton] public class GamePausedSignal : Signal { }
    [Singleton] public class GameResumedSignal : Signal { }

    [Singleton] public class PieceClickedSignal : Signal<PieceView> { }
    [Singleton] public class TileClickedSignal : Signal<Tile> { }

    [Singleton] public class ActionSpawnPieceSignal : Signal<SpawnPieceModel, SocketKey> { }
    [Singleton] public class PieceSpawnedSignal : Signal<PieceSpawnedModel> { }
    [Singleton] public class ActionCancelledSpawnPieceSignal : Signal<SpawnPieceModel, SocketKey> { }

    [Singleton] public class PieceDiedSignal : Signal<PieceModel> { }
    [Singleton] public class ActionPieceDestroyedSignal : Signal<PieceDestroyedModel, SocketKey> { }

    [Singleton] public class ActionDrawCardSignal : Signal<DrawCardModel, SocketKey> { }
    [Singleton] public class ActionCancelledDrawCardSignal : Signal<DrawCardModel, SocketKey> { }
    [Singleton] public class CardDrawnSignal : Signal<CardModel> { }
    [Singleton] public class CardDrawShownSignal : Signal<CardModel> { }

    [Singleton] public class ActionGiveCardSignal : Signal<GiveCardModel, SocketKey> { }
    [Singleton] public class CardGivenSignal : Signal<CardModel> { }

    [Singleton] public class ActionShuffleToDeckSignal : Signal<ShuffleToDeckModel, SocketKey> { }
    [Singleton] public class ShuffleToDeckSignal : Signal<CardModel> { }

    [Singleton] public class ActionDiscardCardSignal : Signal<DiscardCardModel, SocketKey> { }
    [Singleton] public class CardDiscardedSignal : Signal<CardModel> { }

    [Singleton] public class ActionSpawnDeckSignal : Signal<SpawnDeckModel, SocketKey> { }
    [Singleton] public class DeckSpawnedSignal : Signal<SpawnDeckModel> { }

    [Singleton] public class ActionCharmPieceSignal : Signal<CharmPieceModel, SocketKey> { }
    [Singleton] public class PieceCharmedSignal : Signal<CharmPieceModel> { }

    [Singleton] public class ActionUnsummonPieceSignal : Signal<UnsummonPieceModel, SocketKey> { }
    [Singleton] public class PieceUnsummonedSignal : Signal<UnsummonPieceModel> { }

    [Singleton] public class CardSelectedSignal : Signal<CardSelectedModel> { }
    [Singleton] public class CardHoveredSignal : Signal<CardModel> { }
    [Singleton] public class ActivateCardSignal : Signal<ActivateModel> { }
    [Singleton] public class ActionCancelledActivateCardSignal : Signal<ActivateCardModel, SocketKey> { }
    [Singleton] public class ActionActivateCardSignal : Signal<ActivateCardModel> { }
    [Singleton] public class DestroyCardSignal : Signal<int> { }
    [Singleton] public class CardDestroyedSignal : Signal<CardModel> { }

    [Singleton] public class StartChooseSignal : Signal<ChooseModel> { }
    [Singleton] public class CancelChooseSignal : Signal<ChooseModel> { }
    [Singleton] public class UpdateChooseSignal : Signal<ChooseModel> { }
    [Singleton] public class CardChosenSignal : Signal<ChooseModel> { }

    [Singleton] public class NeedsTargetSignal : Signal<CardModel, Tile> { }
    [Singleton] public class StartSelectTargetSignal : Signal<TargetModel> { }
    [Singleton] public class SelectTargetSignal : Signal<TargetModel> { }
    [Singleton] public class UpdateTargetSignal : Signal<TargetModel> { }
    [Singleton] public class CancelSelectTargetSignal : Signal<CardModel> { }

    [Singleton] public class ActivateAbilitySignal : Signal<ActivateAbilityModel> { }
    [Singleton] public class ActionActivateAbilitySignal : Signal<ActivateAbilityModel> { }

    [Singleton] public class StartSelectAbilityTargetSignal : Signal<StartAbilityTargetModel> { }
    [Singleton] public class SelectAbilityTargetSignal : Signal<StartAbilityTargetModel, PieceModel> { }
    [Singleton] public class CancelSelectAbilityTargetSignal : Signal<PieceModel> { }

    [Singleton] public class ActionSetPlayerResourceSignal : Signal<SetPlayerResourceModel> { }
    [Singleton] public class PlayerResourceSetSignal : Signal<SetPlayerResourceModel> { }

    [Singleton] public class PieceTransformedSignal : Signal<TransformPieceModel> { }
    [Singleton] public class ActionTransformPieceSignal : Signal<TransformPieceModel, SocketKey> { }

    [Singleton] public class ActionAttachCodeSignal : Signal<AttachCodeModel, SocketKey> { }

    [Singleton] public class HistoryHoverSignal : Signal<bool> { }
    [Singleton] public class MovePathFoundSignal : Signal<MovePathFoundModel> { }

    [Singleton] public class TauntTilesUpdatedSignal : Signal<TauntTilesUpdateModel> { }

    [Singleton] public class StartGameSettledSignal : Signal { }
}

