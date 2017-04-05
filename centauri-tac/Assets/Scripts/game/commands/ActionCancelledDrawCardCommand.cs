using ctac.signals;
using strange.extensions.command.impl;
using strange.extensions.context.api;
using System.Linq;
using UnityEngine;

namespace ctac
{
    public class ActionCancelledDrawCardCommand : Command
    {
        [Inject(InjectionKeys.GameSignalsRoot)]
        public GameObject contextView { get; set; }

        [Inject]
        public IDebugService debug { get; set; }

        [Inject]
        public ICardService cardService { get; set; }

        [Inject]
        public DrawCardModel cardDraw { get; set; }
        [Inject]
        public SocketKey socketKey { get; set; }

        [Inject] public CardDirectory cardDirectory { get; set; }
        [Inject] public DecksModel decks { get; set; }
        [Inject] public ActionsProcessedModel processedActions { get; set; }

        [Inject] public CardDestroyedSignal cardDestroyed { get; set; }
        [Inject] public AnimationQueueModel animationQueue { get; set; }

        public override void Execute()
        {
            if (!processedActions.Verify(cardDraw.id)) return;

            //No mill animation for now
            if (cardDraw.milled) { return; }

            //assuming overdraw
            if (!cardDraw.overdrew) { return; }

            var newCardModel = cardDirectory.NewFromTemplate(cardDraw.cardId.Value, cardDraw.cardTemplateId.Value, cardDraw.playerId);

            var deckCard = decks.Cards.FirstOrDefault(x => x.playerId == cardDraw.playerId); 
            var cardGameObject = deckCard.gameObject; 
            decks.Cards.Remove(deckCard);
            deckCard.gameObject = null;

            cardService.SetupGameObject(newCardModel, cardGameObject);
            newCardModel.SetCardInPlay(contextView);

            animationQueue.Add(new CardsView.OverdrawCardAnim()
            {
                card = newCardModel,
                cardDestroyed = cardDestroyed,
                animationQueue = animationQueue
            });

            debug.Log(string.Format("Player {0} overdrew card {1} {2}", cardDraw.playerId, cardDraw.cardId, newCardModel.name), socketKey);

        }
    }
}

