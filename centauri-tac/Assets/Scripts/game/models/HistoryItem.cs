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
        public List<HistoryPieceChange> pieceChanges { get; set; }
    }

    public class HistoryPieceChange
    {
        public HistoryPieceChangeType type { get; set; }
        public PieceModel originalPiece { get; set; }
    }

    public enum HistoryPieceChangeType
    {
        HealthChange,
        HistoryBuff,
        Spawner,
        Generic
    }

    public class HistoryHealthChange : HistoryPieceChange
    {
        public PieceHealthChangeModel healthChange { get; set; }
    }

    public class HistoryBuff : HistoryPieceChange
    {
        public PieceBuffModel buff { get; set; }
    }

    public class GenericPieceChange : HistoryPieceChange
    {
    }
}
