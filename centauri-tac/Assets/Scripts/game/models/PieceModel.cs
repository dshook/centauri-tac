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

        public bool hasMoved { get; set; }
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

        public int maxAttacks
        {
            get
            {
                return FlagsHelper.IsSet(statuses, Statuses.DyadStrike) ? 2 : 1;
            }
        }

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

    }

}
