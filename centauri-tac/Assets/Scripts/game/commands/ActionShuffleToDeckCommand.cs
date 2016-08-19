using ctac.signals;
using strange.extensions.command.impl;
using strange.extensions.context.api;
using UnityEngine;

namespace ctac
{
    public class ActionShuffleToDeckCommand : Command
    {
        [Inject(ContextKeys.CONTEXT_VIEW)]
        public GameObject contextView { get; set; }

        [Inject]
        public IDebugService debug { get; set; }
        [Inject]
        public IResourceLoaderService loader { get; set; }

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

            var cardPrefab = loader.Load("Card");

            var cardGameObject = GameObject.Instantiate(
                cardPrefab,
                spawnPosition,
                Quaternion.identity
            ) as GameObject;
            cardGameObject.name = "Player " + cardGiven.playerId + " Card " + cardGiven.cardId;
            newCardModel.rectTransform = cardGameObject.GetComponent<RectTransform>();

            decks.Cards.Add(newCardModel);

            cardGivenSignal.Dispatch(newCardModel);

            debug.Log(string.Format("Player {0} shuffled card {1} {2} into deck", cardGiven.playerId, cardGiven.cardId, newCardModel.name), socketKey);

        }
    }
}

