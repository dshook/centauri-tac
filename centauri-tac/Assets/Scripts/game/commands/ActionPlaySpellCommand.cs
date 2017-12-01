using strange.extensions.command.impl;
using ctac.signals;

namespace ctac
{
    public class ActionPlaySpellCommand : Command
    {
        [Inject]
        public PlaySpellModel cardActivated { get; set; }

        [Inject]
        public ActionsProcessedModel processedActions { get; set; }

        [Inject]
        public SpellPlayedSignal spellPlayed { get; set; }

        public override void Execute()
        {
            if(!processedActions.Verify(cardActivated.id)) return;

            spellPlayed.Dispatch(new SpellPlayedModel() { playSpellAction = cardActivated });
        }
    }
}

