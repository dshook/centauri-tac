using strange.extensions.mediation.impl;
using ctac.signals;
using System.Collections.Generic;

namespace ctac
{
    public class HistoryMediator : Mediator
    {
        [Inject]
        public HistoryView view { get; set; }

        [Inject] public ServerQueueProcessStart qps { get; set; }
        [Inject] public ServerQueueProcessEnd qpc { get; set; }

        [Inject] public PieceSpawnedSignal spawnPiece { get; set; }

        private List<HistoryItem> history = new List<HistoryItem>();

        public override void OnRegister()
        {
            view.init();
            spawnPiece.AddListener(onSpawnPiece);
        }

        public override void onRemove()
        {
            spawnPiece.RemoveListener(onSpawnPiece);
        }

        private void onSpawnPiece(PieceModel piece)
        {
            pushHistory(new HistoryItem()
            {
                type = HistoryItemType.MinionPlayed,
                initiatingPlayerId = piece.playerId,
                piecesAffected = new List<PieceModel>() { piece }
            });
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

