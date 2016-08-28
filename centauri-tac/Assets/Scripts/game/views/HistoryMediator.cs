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

        [Inject] public ServerQueueProcessStart qps { get; set; }
        [Inject] public ServerQueueProcessEnd qpc { get; set; }

        [Inject] public PieceSpawnedSignal spawnPiece { get; set; }
        [Inject] public ActionPlaySpellSignal spellPlayed { get; set; }

        [Inject] public TurnEndedSignal turnEnded { get; set; }

        private List<HistoryItem> history = new List<HistoryItem>();
        private HistoryItem currentItem = null;

        public override void OnRegister()
        {
            view.init();
            qpc.AddListener(onQpc);
            spawnPiece.AddListener(onSpawnPiece);
            spellPlayed.AddListener(onSpellPlayed);
            turnEnded.AddListener(onTurnEnd);
        }

        public override void onRemove()
        {
            qpc.RemoveListener(onQpc);
            spawnPiece.RemoveListener(onSpawnPiece);
            spellPlayed.RemoveListener(onSpellPlayed);
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
                piecesAffected = new List<PieceModel>(),
                cardsAffected = new List<CardModel>()
            };
        }

        private void onSpawnPiece(PieceModel piece)
        {
            if(piece.isHero) return;

            if (currentItem == null)
            {
                CreateCurrent(HistoryItemType.MinionPlayed, piece.playerId);
            }
            currentItem.piecesAffected.Add(piece);
        }

        private void onSpellPlayed(PlaySpellModel spell)
        {
            if (currentItem == null)
            {
                CreateCurrent(HistoryItemType.CardPlayed, spell.playerId);
            }
            //currentItem.cardsAffected.Add(spell.cardInstanceId);
        }

        private void onTurnEnd(GameTurnModel turn)
        {
            //for hotseat mode update the colors each turn to reflect the current player
            if (players.OpponentId(turn.currentPlayerId) != turn.currentPlayerId)
            {
                view.UpdatePlayerColors();
            }
        }

        private void pushHistory(HistoryItem h)
        {
            history.Add(h);
            view.AddItem(h);
        }

        public class HistoryItem
        {
            public HistoryItemType type { get; set; }
            public int initiatingPlayerId { get; set; }
            public List<PieceModel> piecesAffected { get; set; }
            public List<CardModel> cardsAffected { get; set; }

        }

        public enum HistoryItemType
        {
            Attack,
            MinionPlayed,
            CardPlayed,
            Event
        }
    }
}

