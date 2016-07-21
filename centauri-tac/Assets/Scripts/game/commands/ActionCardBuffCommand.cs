using strange.extensions.command.impl;
using ctac.signals;
using System.Linq;

namespace ctac
{
    public class ActionCardBuffCommand : Command
    {
        [Inject]
        public SocketKey socketKey { get; set; }

        [Inject]
        public CardBuffModel cardBuff { get; set; }

        [Inject]
        public CardBuffSignal cardBuffedSignal { get; set; }

        [Inject]
        public CardsModel cards { get; set; }

        [Inject]
        public ActionsProcessedModel processedActions { get; set; }

        [Inject]
        public IDebugService debug { get; set; }

        public override void Execute()
        {
            if (!processedActions.Verify(cardBuff.id)) return;

            var card = cards.Card(cardBuff.cardId);

            if (card == null)
            {
                debug.LogWarning("Could not find card to buff " + cardBuff.cardId, socketKey);
                return;
            }
            if (cardBuff.removed)
            {
                var buff = card.buffs.FirstOrDefault(x => x.name == cardBuff.name);
                card.buffs.Remove(buff);
            }
            else
            {
                card.buffs.Add(cardBuff);
            }

            card.cost = cardBuff.newCost ?? card.cost;

            cardBuffedSignal.Dispatch(cardBuff);

            debug.Log( string.Format("Card {0} buffed with {1}", cardBuff.cardId, cardBuff.name), socketKey );
        }
    }
}

