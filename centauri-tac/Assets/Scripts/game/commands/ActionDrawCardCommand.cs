using ctac.signals;
using strange.extensions.command.impl;
using strange.extensions.context.api;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ctac
{
    public class ActionDrawCardCommand : Command
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
        public CardDrawnSignal cardDrawn { get; set; }

        [Inject]
        public CardDirectory cardDirectory { get; set; }

        [Inject]
        public DecksModel decks { get; set; }

        [Inject]
        public SocketKey socketKey { get; set; }

        [Inject]
        public ActionsProcessedModel processedActions { get; set; }

        public override void Execute()
        {
            if (!processedActions.Verify(cardDraw.id)) return;

            var newCardModel = cardDirectory.NewFromTemplate(cardDraw.cardId.Value, cardDraw.cardTemplateId.Value, cardDraw.playerId);

            if (!decks.Cards.Any(x => x.playerId == cardDraw.playerId))
            {
                debug.LogError("Cannot draw card from empty deck", socketKey);
                return;
            }

            var deckCard = decks.Cards.FirstOrDefault(x => x.playerId == cardDraw.playerId); 
            var cardGameObject = deckCard.gameObject; 
            decks.Cards.Remove(deckCard);
            deckCard.gameObject = null;

            cardService.SetupGameObject(newCardModel, cardGameObject);
            newCardModel.SetCardInPlay(contextView);

            cardDrawn.Dispatch(newCardModel);

            debug.Log(string.Format("Player {0} drew card {1} {2}", cardDraw.playerId, cardDraw.cardId, newCardModel.name), socketKey);

        }
    }
}

