using ctac.signals;
using strange.extensions.command.impl;
using System.Linq;

namespace ctac
{
    public class ActionDiscardCardCommand : Command
    {
        [Inject]
        public IDebugService debug { get; set; }

        [Inject]
        public DiscardCardModel cardDiscarded { get; set; }

        [Inject]
        public CardDiscardedSignal cardDiscardedSignal { get; set; }

        [Inject]
        public CardDirectory cardDirectory { get; set; }

        [Inject]
        public CardsModel cards { get; set; }

        [Inject]
        public SocketKey socketKey { get; set; }

        [Inject]
        public ActionsProcessedModel processedActions { get; set; }

        public override void Execute()
        {
            if (!processedActions.Verify(cardDiscarded.id)) return;

            var discarded = cards.Cards.FirstOrDefault(c => c.id == cardDiscarded.cardId);

            cardDiscardedSignal.Dispatch(discarded);

            debug.Log(string.Format("Player {0} discarded card {1} {2}", cardDiscarded.playerId, cardDiscarded.cardId, discarded.name), socketKey);

        }
    }
}

