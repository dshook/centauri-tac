using strange.extensions.mediation.impl;
using ctac.signals;
using System.Collections.Generic;

namespace ctac
{
    public class HistoryMediator : Mediator
    {
        [Inject]
        public HistoryView view { get; set; }

        [Inject] public GamePlayersModel players { get; set; }
        [Inject] public PiecesModel pieces { get; set; }
        [Inject] public CardsModel cards { get; set; }

        [Inject] public ServerQueueProcessEnd qpc { get; set; }
        [Inject] public ActionPassTurnSignal passTurn { get; set; }

        [Inject] public PieceSpawnedSignal spawnPiece { get; set; }
        [Inject] public ActionPlaySpellSignal spellPlayed { get; set; }
        [Inject] public PieceAttackedSignal pieceAttacked { get; set; }
        [Inject] public PieceHealthChangedSignal pieceHealthChange { get; set; }

        [Inject] public TurnEndedSignal turnEnded { get; set; }

        private List<HistoryItem> history = new List<HistoryItem>();
        private HistoryItem currentItem = null;

        public override void OnRegister()
        {
            view.init();
            qpc.AddListener(onQpc);
            spawnPiece.AddListener(onSpawnPiece);
            spellPlayed.AddListener(onSpellPlayed);
            pieceAttacked.AddListener(onPieceAttacked);
            pieceHealthChange.AddListener(onPieceHealthChanged);
            turnEnded.AddListener(onTurnEnd);
        }

        public override void onRemove()
        {
            qpc.RemoveListener(onQpc);
            spawnPiece.RemoveListener(onSpawnPiece);
            spellPlayed.RemoveListener(onSpellPlayed);
            pieceAttacked.RemoveListener(onPieceAttacked);
            pieceHealthChange.RemoveListener(onPieceHealthChanged);
            turnEnded.RemoveListener(onTurnEnd);
        }

        //Generally how the history processor work is by listening for relavent events as they come in
        //If they're the first in the queue process then they'll make a new current item, otherwise they'll
        //attach themselves to the current one.  Then when the queue processing is complete we'll push the
        //built up item
        private void onQpc(int t)
        {
            if (currentItem != null)
            {
                pushHistory(currentItem);
                currentItem = null;
            }
        }

        private void CreateCurrent(HistoryItemType type, int player)
        {
            currentItem = new HistoryItem()
            {
                type = type,
                initiatingPlayerId = player,
                healthChanges = new List<HistoryHealthChange>()
            };
        }

        //set up an event action here for any timers that trigger stuff between the turns
        private void onPassTurn(PassTurnModel pass)
        {
            if (currentItem == null)
            {
                //CreateCurrent(HistoryItemType.Event, );
            }
        }

        private void onSpawnPiece(PieceModel piece)
        {
            if(piece.isHero) return;

            if (currentItem == null)
            {
                CreateCurrent(HistoryItemType.MinionPlayed, piece.playerId);
            }
            currentItem.triggeringPiece = CopyPiece(piece);
        }

        private void onSpellPlayed(PlaySpellModel spell)
        {
            if (currentItem == null)
            {
                CreateCurrent(HistoryItemType.CardPlayed, spell.playerId);
            }
            var card = cards.Card(spell.cardInstanceId);
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

        private void onPieceAttacked(AttackPieceModel atk)
        {
            var attacker = pieces.Piece(atk.attackingPieceId);
            if (currentItem == null)
            {
                CreateCurrent(HistoryItemType.Attack, attacker.playerId);
            }
            currentItem.triggeringPiece = attacker;
        }

        private void onPieceHealthChanged(PieceHealthChangeModel phcm)
        {
            //should be encapsulated under some other event
            if (currentItem == null)
            {
                return;
            }

            var piece = pieces.Piece(phcm.pieceId);
            currentItem.healthChanges.Add(new HistoryHealthChange()
            {
                originalPiece = piece,
                healthChange = phcm
            });
        }

        private void pushHistory(HistoryItem h)
        {
            history.Add(h);
            view.AddItem(h);
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

