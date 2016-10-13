using strange.extensions.mediation.impl;
using ctac.signals;
using System.Collections.Generic;
using System.Linq;

namespace ctac
{
    public class HistoryMediator : Mediator
    {
        [Inject] public HistoryView view { get; set; }

        [Inject] public GamePlayersModel players { get; set; }
        [Inject] public PiecesModel pieces { get; set; }
        [Inject] public CardsModel cards { get; set; }
        [Inject] public PossibleActionsModel possibleActions { get; set; }

        [Inject] public ServerQueueProcessEnd qpc { get; set; }

        [Inject] public PieceSpawnedSignal spawnPiece { get; set; }
        [Inject] public SpellPlayedSignal spellPlayed { get; set; }
        [Inject] public PieceAttackedSignal pieceAttacked { get; set; }
        [Inject] public PieceBuffSignal pieceBuff { get; set; }
        [Inject] public PieceHealthChangedSignal pieceHealthChange { get; set; }

        [Inject] public TurnEndedSignal turnEnded { get; set; }

        [Inject] public IDebugService debug { get; set; }

        private List<HistoryItem> history = new List<HistoryItem>();
        private Dictionary<int?, HistoryItem> currentItems = new Dictionary<int?, HistoryItem>();

        public override void OnRegister()
        {
            view.init();
            qpc.AddListener(onQpc);
            spawnPiece.AddListener(onSpawnPiece);
            spellPlayed.AddListener(onSpellPlayed);
            pieceAttacked.AddListener(onPieceAttacked);
            pieceHealthChange.AddListener(onPieceHealthChanged);
            pieceBuff.AddListener(onPieceBuff);
            turnEnded.AddListener(onTurnEnd);
        }

        public override void onRemove()
        {
            qpc.RemoveListener(onQpc);
            spawnPiece.RemoveListener(onSpawnPiece);
            spellPlayed.RemoveListener(onSpellPlayed);
            pieceAttacked.RemoveListener(onPieceAttacked);
            pieceHealthChange.RemoveListener(onPieceHealthChanged);
            pieceBuff.AddListener(onPieceBuff);
            turnEnded.RemoveListener(onTurnEnd);
        }

        //Generally how the history processor work is by listening for relavent events as they come in
        //If they're the first in the queue process then they'll make a new current item, otherwise they'll
        //attach themselves to the current one.  Then when the queue processing is complete we'll push the
        //built up item
        private void onQpc(int t)
        {
            if (currentItems.Count > 0)
            {
                pushHistory(currentItems.Values.ToList());
                currentItems.Clear();
            }
        }

        /// <summary>
        /// Get or create the current history item for the activating piece Id which the history items are grouped by
        /// If one doesn't exist, create it with the player and type passed
        /// </summary>
        private HistoryItem GetCurrent(int? activatingPieceId, HistoryItemType type, int player)
        {
            if (!activatingPieceId.HasValue)
            {
                //Dictionary can't store null values as keys so hack it to sentinal
                activatingPieceId = -1;
            }
            if (currentItems.ContainsKey(activatingPieceId))
            {
                return currentItems[activatingPieceId];
            }
            var currentItem = new HistoryItem()
            {
                type = type,
                initiatingPlayerId = player,
                spellDamage = possibleActions.GetSpellDamage(player),
                pieceChanges = new List<HistoryPieceChange>(),
            };
            currentItems[activatingPieceId] = currentItem;
            return currentItem;
        }

        private void onSpawnPiece(PieceSpawnedModel pieceSpawned)
        {
            if(pieceSpawned.piece.isHero) return;

            var currentItem = GetCurrent(
                pieceSpawned.spawnPieceAction.activatingPieceId, 
                HistoryItemType.MinionPlayed, 
                pieceSpawned.piece.playerId
            );

            currentItem.triggeringPiece = CopyPiece(pieceSpawned.piece);
        }

        private void onSpellPlayed(SpellPlayedModel spellPlayed)
        {
            var currentItem = GetCurrent(
                spellPlayed.playSpellAction.activatingPieceId, 
                HistoryItemType.CardPlayed, 
                spellPlayed.playSpellAction.playerId
            );
            var card = cards.Card(spellPlayed.playSpellAction.cardInstanceId);
            currentItem.triggeringCard = CopyCard(card);
        }

        private void onTurnEnd(GameTurnModel turn)
        {
            //for hotseat mode update the colors each turn to reflect the current player
            if (players.isHotseat)
            {
                view.UpdatePlayerColors();
            }
        }

        private void onPieceBuff(PieceBuffModel pieceBuff)
        {
            var piece = CopyPiece(pieces.Piece(pieceBuff.pieceId));
            var currentItem = GetCurrent(pieceBuff.activatingPieceId, HistoryItemType.Event, piece.playerId);

            if (currentItem.triggeringPiece == null && pieceBuff.activatingPieceId.HasValue)
            {
                currentItem.triggeringPiece = CopyPiece(pieces.Piece(pieceBuff.activatingPieceId.Value));
            }

            currentItem.pieceChanges.Add(new HistoryBuff()
            {
                type = HistoryPieceChangeType.HistoryBuff,
                originalPiece = piece,
                buff = pieceBuff
            });
        }

        private void onPieceAttacked(AttackPieceModel atk)
        {
            var attacker = pieces.Piece(atk.attackingPieceId);
            var currentItem = GetCurrent(atk.activatingPieceId, HistoryItemType.Attack, attacker.playerId);
            currentItem.triggeringPiece = attacker;
        }

        private void onPieceHealthChanged(PieceHealthChangeModel phcm)
        {
            var piece = CopyPiece(pieces.Piece(phcm.pieceId));
            //should be encapsulated under some other event, but could be from an event or deathrattle
            var currentItem = GetCurrent(phcm.activatingPieceId, HistoryItemType.Event, piece.playerId);

            if (currentItem.triggeringPiece == null && phcm.activatingPieceId.HasValue)
            {
                currentItem.triggeringPiece = CopyPiece(pieces.Piece(phcm.activatingPieceId.Value));
            }

            currentItem.pieceChanges.Add(new HistoryHealthChange()
            {
                type = HistoryPieceChangeType.HealthChange,
                originalPiece = piece,
                healthChange = phcm
            });
        }

        private void pushHistory(List<HistoryItem> items)
        {
            history.AddRange(items);
            foreach (var item in items)
            {
                view.AddItem(item);
            }
        }

        private PieceModel CopyPiece(PieceModel src)
        {
            var dst = new PieceModel();
            src.CopyProperties(dst);
            dst.gameObject = null;  //don't need the original game object here
            return dst;
        }

        private CardModel CopyCard(CardModel src)
        {
            var dst = new CardModel();
            src.CopyProperties(dst);
            dst.gameObject = null;  //don't need the original game object here
            return dst;
        }

    }
}

