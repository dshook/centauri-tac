namespace ctac
{
    public class PieceBuffModel : BaseAction
    {
        public int pieceId { get; set; }
        public int buffId { get; set; }
        public string name { get; set; }
        public bool removed { get; set; }

        public int? attack { get; set; }
        public int? health { get; set; }
        public int? movement { get; set; }
        public int? range { get; set; }

        public int? newAttack { get; set; }
        public int? newHealth { get; set; }
        public int? newMovement { get; set; }
        public int? newRange { get; set; }

        public Statuses? addStatus { get; set; }
        public Statuses? removeStatus { get; set; }

        public Statuses statuses { get; set; }

        //For buff stat update that might not have everything set
        public void CopyBuff(PieceBuffModel src){
            attack = src.attack;
            health = src.health;
            movement = src.movement;
            range = src.range;

            newAttack = src.newAttack;
            newHealth = src.newHealth;
            newMovement = src.newMovement;
            newRange = src.newRange;

            statuses = src.statuses;
        }
    }
}
