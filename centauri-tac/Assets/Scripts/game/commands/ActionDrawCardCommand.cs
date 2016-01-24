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
        [Inject(ContextKeys.CONTEXT_VIEW)]
        public GameObject contextView { get; set; }

        [Inject]
        public IDebugService debug { get; set; }

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

            var cardTemplate = cardDirectory.Card(cardDraw.cardTemplateId);

            var newCardModel = new CardModel()
                {
                    id = cardDraw.cardId,
                    cardTemplateId = cardDraw.cardTemplateId,
                    playerId = cardDraw.playerId,
                    name = cardTemplate.name,
                    description = cardTemplate.description,
                    cost = cardTemplate.cost,
                    attack = cardTemplate.attack,
                    health = cardTemplate.health,
                    movement = cardTemplate.movement,
                    tags = cardTemplate.tags,
                    playable = false
                };

            if (!decks.Cards.Any(x => x.playerId == cardDraw.playerId))
            {
                debug.LogError("Cannot draw card from empty deck", socketKey);
                return;
            }

            var deckCard = decks.Cards.FirstOrDefault(x => x.playerId == cardDraw.playerId); 
            var cardGameObject = deckCard.gameObject; 
            decks.Cards.Remove(deckCard);
            deckCard.gameObject = null;

            var cardParent = contextView.transform.FindChild(Constants.cardCanvas);
            cardGameObject.transform.SetParent(cardParent.transform);

            newCardModel.gameObject = cardGameObject;
            var cardView = cardGameObject.AddComponent<CardView>();
            cardView.card = newCardModel;
            newCardModel.cardView = cardView;

            cardDrawn.Dispatch(newCardModel);

            debug.Log(string.Format("Player {0} drew card {1} {2}", cardDraw.playerId, cardDraw.cardId, newCardModel.name), socketKey);

        }
    }
}

