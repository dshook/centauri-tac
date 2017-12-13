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

        [Inject] public DrawCardModel cardDraw { get; set; }
        [Inject] public SocketKey socketKey { get; set; }

        [Inject] public CardDrawnSignal cardDrawn { get; set; }

        [Inject] public CardDirectory cardDirectory { get; set; }
        [Inject] public DecksModel decks { get; set; }
        [Inject] public CardsModel cards { get; set; }

        [Inject] public ActionsProcessedModel processedActions { get; set; }

        [Inject] public IDebugService debug { get; set; }
        [Inject] public ICardService cardService { get; set; }

        public override void Execute()
        {
            //Need to double process this one because of the cardTemplateId only being sent to the player who drew the card
            //if (!processedActions.Verify(cardDraw.id)) return;

            CardModel newCardModel = null;

            //first check to see if this cardId already exists for local dev mode.
            //this will happen since the single client receives both draw card messages and just needs to update the enemy card in that case
            var existingCard = cards.Card(cardDraw.cardId.Value);
            if(existingCard != null){
                if (cardDraw.cardTemplateId.HasValue) {
                    cardService.CopyCard(cardDirectory.Card(cardDraw.cardTemplateId.Value), existingCard);
                    cardService.UpdateCardArt(existingCard);
                    existingCard.playerId = cardDraw.playerId;
                }
                return;
            }

            //If the card is for us
            if(cardDraw.cardTemplateId.HasValue){
                newCardModel = cardDirectory.NewFromTemplate(cardDraw.cardId.Value, cardDraw.cardTemplateId.Value, cardDraw.playerId);
            }else{
            //enemy card
                newCardModel = new CardModel(){
                    id = cardDraw.cardId.Value,
                    playerId = cardDraw.playerId,
                    tags = new List<string>(),
                    buffs = new List<CardBuffModel>()
                };
            }

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

            cards.Cards.Add(newCardModel);
            cardDrawn.Dispatch(newCardModel);

            debug.Log(string.Format("Player {0} drew card {1} {2}", cardDraw.playerId, cardDraw.cardId, newCardModel.name), socketKey);

        }
    }
}

