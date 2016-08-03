using System.Linq;

namespace ctac
{
    public class ChooseModel
    {
        public CardModel choosingCard { get; set; }

        public ChoiceCard choices { get; set; }

        public int? chosenTemplateId { get; set; }

        //if choose also needs a target
        public PieceModel selectedPiece { get; set; }

        public bool chooseFulfilled
        {
            get
            {
                if (chosenTemplateId.HasValue)
                {
                    var selected = choices.choices.FirstOrDefault(x => x.cardTemplateId == chosenTemplateId.Value);
                    if (selected == null)
                    {
                        return false;
                    }
                    if (selected.targets != null 
                        && selected.targets.targetPieceIds != null 
                        && selected.targets.targetPieceIds.Count > 0
                    )
                    {
                        return selectedPiece != null && selected.targets.targetPieceIds.Contains(selectedPiece.id);
                    }
                    return true;
                }

                return false;
            }
        }
    }
}
