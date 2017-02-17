using ctac.signals;
using strange.extensions.command.impl;

namespace ctac
{
    public class ActionCancelledPlaySpellCommand : Command
    {
        [Inject] public PlaySpellModel spellPlayed { get; set; }

        [Inject] public CardsModel cards { get; set; }
        [Inject] public CancelSelectTargetSignal cancelSelectTarget { get; set; }
        [Inject] public CardSelectedSignal cardSelected { get; set; }

        [Inject]
        public IDebugService debug { get; set; }

        public override void Execute()
        {
            cardSelected.Dispatch(null);
            //reactivate card
            var card = cards.Card(spellPlayed.cardInstanceId);
            if (card != null)
            {
                debug.Log("Cancelling spell cast");
                card.activated = false;
                cancelSelectTarget.Dispatch(card);
            }
            else
            {
                debug.Log("Could not find spell to cancel " + spellPlayed.cardInstanceId);
            }
        }
    }
}

