using strange.extensions.command.impl;
using strange.extensions.context.api;
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
        public CardsModel cards { get; set; }

        [Inject]
        public CardDirectory cardDirectory { get; set; }

        [Inject]
        public SocketKey socketKey { get; set; }

        [Inject]
        public ActionsProcessedModel processedActions { get; set; }

        public override void Execute()
        {
            //check to see if this action has already been processed by another player
            if (processedActions.processedActions.Any(x => x == cardDraw.id))
            {
                return;
            }

            var cardPrefab = Resources.Load("Card") as GameObject;
            var cardParent = contextView.transform.FindChild("cardCanvas");
            var cardTemplate = cardDirectory.directory.FirstOrDefault(c => c.id == cardDraw.cardId);

            var newCardModel = new CardModel()
                {
                    id = cardDraw.cardId,
                    playerId = cardDraw.playerId,
                    name = cardTemplate.name,
                    description = cardTemplate.description,
                    attack = cardTemplate.attack,
                    health = cardTemplate.health
                };

            cards.Cards.Add(newCardModel );

            var newCard = GameObject.Instantiate(
                cardPrefab,
                Vector3.zero,
                Quaternion.identity
            ) as GameObject;
            newCard.transform.SetParent(cardParent.transform);
            newCard.transform.localPosition = Vector3.zero;
            newCard.transform.localScale = Vector3.one;

            newCardModel.gameObject = newCard;
            var pieceView = newCard.AddComponent<CardView>();
            pieceView.card = newCardModel;

            debug.Log(string.Format("Player {0} drew card {1} {2}", cardDraw.playerId, cardDraw.cardId, newCardModel.name), socketKey);

        }
    }
}

