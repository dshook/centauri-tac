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
        [Inject] public TurnEndedSignal turnEnded { get; set; }

        private List<HistoryItem> history = new List<HistoryItem>();

        public override void OnRegister()
        {
            view.init();
            spawnPiece.AddListener(onSpawnPiece);
            turnEnded.AddListener(onTurnEnd);
        }

        public override void onRemove()
        {
            spawnPiece.RemoveListener(onSpawnPiece);
            turnEnded.RemoveListener(onTurnEnd);
        }

        private void onSpawnPiece(PieceModel piece)
        {
            if(piece.isHero) return;

            pushHistory(new HistoryItem()
            {
                type = HistoryItemType.MinionPlayed,
                initiatingPlayerId = piece.playerId,
                piecesAffected = new List<PieceModel>() { piece }
            });
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

