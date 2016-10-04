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
        public int? activatingPieceId { get; set; }
        public int spellDamage { get; set; } //what the spell damage was at the time
        public PieceModel triggeringPiece { get; set; }
        public CardModel triggeringCard { get; set; }
        public List<HistoryHealthChange> healthChanges { get; set; }
    }

    public class HistoryHealthChange
    {
        public PieceModel originalPiece { get; set; }
        public PieceHealthChangeModel healthChange { get; set; }
    }
}
