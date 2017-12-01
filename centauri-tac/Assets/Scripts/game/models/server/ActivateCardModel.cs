using Newtonsoft.Json;

namespace ctac
{
    public class ActivateCardModel : BaseAction
    {
        public int playerId { get; set; }
        public int cardInstanceId { get; set; }
        public PositionModel position { get; set; }
        public PositionModel pivotPosition { get; set; }
        public int? targetPieceId { get; set; }
        public int? chooseCardTemplateId { get; set; }

        //filled out by server
        public int? spellDamage { get; set; }

        [JsonIgnore]
        public CardModel card { get; set; }
    }
}
