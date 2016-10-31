using ctac.signals;
using strange.extensions.command.impl;
using strange.extensions.context.api;
using UnityEngine;

namespace ctac
{
    public class ActionShuffleToDeckCommand : Command
    {
        [Inject]
        public IDebugService debug { get; set; }
        [Inject]
        public ICardService cardService { get; set; }

        [Inject]
        public ShuffleToDeckModel cardGiven { get; set; }

        [Inject]
        public ShuffleToDeckSignal cardGivenSignal { get; set; }

        [Inject]
        public CardDirectory cardDirectory { get; set; }

        [Inject]
        public SocketKey socketKey { get; set; }

        [Inject]
        public DecksModel decks { get; set; }

        [Inject]
        public ActionsProcessedModel processedActions { get; set; }

        private Vector3 spawnPosition = new Vector3(10000, 10000, 0);

        public override void Execute()
        {
            if (!processedActions.Verify(cardGiven.id)) return;

            var newCardModel = cardDirectory.NewFromTemplate(cardGiven.cardId, cardGiven.cardTemplateId, cardGiven.playerId);

            var DeckGO = GameObject.Find("Deck");
            cardService.CreateCard(newCardModel, DeckGO.transform, spawnPosition);

            decks.Cards.Add(newCardModel);

            cardGivenSignal.Dispatch(newCardModel);

            debug.Log(string.Format("Player {0} shuffled card {1} {2} into deck", cardGiven.playerId, cardGiven.cardId, newCardModel.name), socketKey);

        }
    }
}

