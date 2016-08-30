using System.Collections.Generic;

namespace ctac
{
    public enum HistoryItemType
    {
        Attack,
        MinionPlayed,
        CardPlayed,
        Event
    }

    public class HistoryItem
    {
        public HistoryItemType type { get; set; }
        public int initiatingPlayerId { get; set; }
        public List<PieceModel> piecesAffected { get; set; }
        public List<CardModel> cardsAffected { get; set; }
    }
}
