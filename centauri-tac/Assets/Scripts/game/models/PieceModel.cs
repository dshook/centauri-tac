using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ctac
{
    public class PieceModel
    {
        public int id { get; set; }
        public int cardTemplateId { get; set; }
        public int playerId { get; set; }
        public string name { get; set; }

        public GameObject gameObject { get; set; }

        public bool currentPlayerHasControl { get; set; }

        public int moveCount { get; set; }
        public bool isMoving { get; set; }
        public int attackCount { get; set; }

        public List<string> tags { get; set; }
        public Statuses statuses { get; set; }

        public Direction direction { get; set; }

        //canonical game stat position of where the unit is
        public Vector2 tilePosition { get; set; }

        public int attack { get; set; }
        public int health { get; set; }
        public int movement { get; set; }
        public int? range { get; set; }
        public int armor { get; set; }

        public int baseAttack { get; set; }
        public int baseHealth { get; set; }
        public int baseMovement { get; set; }
        public int? baseRange { get; set; }

        public List<PieceBuffModel> buffs { get; set; }

        public bool hasDeathEvent { get; set; }
        public bool hasEvent { get; set; }
        public bool hasAura { get; set; }
        public bool isSelected { get; set; }

        [JsonIgnore]
        public CardSets cardSet { get; set; }

        [JsonIgnore] //in turn-years, but gets reset on charm
        public int age { get; set; }

        [JsonIgnore]
        public int maxAttacks
        {
            get
            {
                return FlagsHelper.IsSet(statuses, Statuses.DyadStrike) ? 2 : 1;
            }
        }

        [JsonIgnore]
        public int maxBuffedHealth
        {
            get
            {
                return baseHealth + (buffs.Sum(s => s.health) ?? 0);
            }
        }

        [JsonIgnore]
        public bool isMinion
        {
            get
            {
                return tags.Contains("Minion");
            }
        }

        [JsonIgnore]
        public bool isHero
        {
            get
            {
                return tags.Contains("Hero");
            }
        }

        [JsonIgnore]
        public bool isMelee
        {
            get
            {
                return !range.HasValue;
            }
        }

        [JsonIgnore]
        public bool isRanged
        {
            get
            {
                return range.HasValue;
            }
        }

        [JsonIgnore]
        public bool canAttack
        {
            get
            {
                return attack > 0
                    && attackCount < maxAttacks
                    && !FlagsHelper.IsSet(statuses, Statuses.CantAttack)
                    && !FlagsHelper.IsSet(statuses, Statuses.Paralyze);
            }
        }

        [JsonIgnore]
        public bool canMove
        {
            get
            {
                return movement > 0
                    && moveCount < movement
                    && !FlagsHelper.IsSet(statuses, Statuses.Root)
                    && !FlagsHelper.IsSet(statuses, Statuses.Paralyze);
            }
        }

        [JsonIgnore]
        private PieceView _pieceView;

        [JsonIgnore]
        public PieceView pieceView
        {
            get
            {
                if (_pieceView == null && gameObject != null)
                {
                    _pieceView = gameObject.GetComponent<PieceView>();
                }
                return _pieceView;
            }
        }

        //What a lame signature
        public void UpdateStatuses(Statuses? add, Statuses? remove, Statuses newStatuses, int? newRange, out Statuses added, out Statuses removed)
        {
            added = add ?? Statuses.None;
            removed = remove ?? Statuses.None;

            if (range.HasValue && newRange.HasValue && newRange.Value == 0)
            {
                removed = Statuses.isRanged;
            }
            if ((!range.HasValue || range == 0) && newRange.HasValue && newRange > 0)
            {
                added = Statuses.isRanged;
            }

            //if silenced we have to also remove the event client statuses, don't think we need to remove the event ones though
            //since they should get cleared from possible actions
            if ((add & Statuses.Silence) != 0)
            {
                if (hasAura)
                {
                    FlagsHelper.Set(ref removed, Statuses.hasAura);
                    hasAura = false;
                }

            }

            statuses = newStatuses;
        }

    }

}
