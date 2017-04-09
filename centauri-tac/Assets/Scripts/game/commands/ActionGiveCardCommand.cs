using ctac.signals;
using strange.extensions.command.impl;
using strange.extensions.context.api;
using UnityEngine;

namespace ctac
{
    public class ActionGiveCardCommand : Command
    {
        [Inject(InjectionKeys.GameSignalsRoot)]
        public GameObject contextView { get; set; }

        [Inject]
        public IDebugService debug { get; set; }
        [Inject]
        public ICardService cardService { get; set; }

        [Inject]
        public GiveCardModel cardGiven { get; set; }

        [Inject]
        public CardGivenSignal cardGivenSignal { get; set; }

        [Inject]
        public CardDirectory cardDirectory { get; set; }

        [Inject]
        public SocketKey socketKey { get; set; }

        [Inject]
        public ActionsProcessedModel processedActions { get; set; }

        private Vector3 spawnPosition = new Vector3(10000, 10000, 0);

        public override void Execute()
        {
            if (!processedActions.Verify(cardGiven.id)) return;

            var newCardModel = cardDirectory.NewFromTemplate(cardGiven.cardId, cardGiven.cardTemplateId, cardGiven.playerId);

            cardService.CreateCard(newCardModel, null, spawnPosition);

            cardService.SetupGameObject(newCardModel, newCardModel.gameObject);
            newCardModel.SetCardInPlay(contextView);

            cardGivenSignal.Dispatch(newCardModel);

            debug.Log(string.Format("Player {0} was given card {1} {2}", cardGiven.playerId, cardGiven.cardId, newCardModel.name), socketKey);

        }
    }
}

