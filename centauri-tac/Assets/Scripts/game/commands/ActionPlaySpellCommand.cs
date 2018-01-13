using strange.extensions.command.impl;
using ctac.signals;

namespace ctac
{
    public class ActionPlaySpellCommand : Command
    {
        [Inject] public PlaySpellModel cardActivated { get; set; }

        [Inject] public ActionsProcessedModel processedActions { get; set; }

        [Inject] public CardActivatedSignal cardActivatedSignal { get; set; }
        [Inject] public SpellPlayedSignal spellPlayed { get; set; }

        [Inject] public CardDirectory cardDirectory { get; set; }
        [Inject] public CardsModel cards { get; set; }
        [Inject] public ICardService cardService { get; set; }

        public override void Execute()
        {
            if(!processedActions.Verify(cardActivated.id)) return;

            //First activate the card that played the spell
            var cardActivatedModel = cardService.ActivateCardInstance(cards, cardDirectory, cardActivated.cardInstanceId, cardActivated.cardTemplateId, cardActivated.spellDamage);

            cardActivatedSignal.Dispatch(cardActivatedModel);

            spellPlayed.Dispatch(new SpellPlayedModel() { playSpellAction = cardActivated });
        }
    }
}

