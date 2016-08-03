using strange.extensions.command.impl;
using strange.extensions.context.api;
using UnityEngine;

namespace ctac
{

    public class StartChooseCommand : Command
    {
        [Inject]
        public ChooseModel chooseModel { get; set; }

        [Inject(ContextKeys.CONTEXT_VIEW)]
        public GameObject contextView { get; set; }

        [Inject]
        public IDebugService debug { get; set; }

        [Inject]
        public CardDirectory cardDirectory { get; set; }

        private Vector2 anchorPosition = new Vector2(0.5f, 0.5f);
        private Vector3 rightSpawnPosition = new Vector3(0, 0, 0);

        public override void Execute()
        {
            var leftChoice = chooseModel.choices.choices[0];
            var rightChoice = chooseModel.choices.choices[1];

            //create new card models based on the template id's, with fake real id's since they're not real cards
            var leftCardModel = cardDirectory.NewFromTemplate(998, leftChoice.cardTemplateId, chooseModel.choosingCard.playerId);
            var rightCardModel = cardDirectory.NewFromTemplate(998, rightChoice.cardTemplateId, chooseModel.choosingCard.playerId);

            var cardPrefab = Resources.Load("Card") as GameObject;

            var leftGameObject = GameObject.Instantiate(
                cardPrefab,
                rightSpawnPosition,
                Quaternion.identity
            ) as GameObject;
            leftGameObject.name = "Left Choice Card ";
            var rightGameObject = GameObject.Instantiate(
                cardPrefab,
                rightSpawnPosition,
                Quaternion.identity
            ) as GameObject;
            rightGameObject.name = "Right Choice Card ";

            leftCardModel.SetupGameObject(leftGameObject);
            leftCardModel.SetCardInPlay(contextView);

            rightCardModel.SetupGameObject(rightGameObject);
            rightCardModel.SetCardInPlay(contextView);

            SetCardXPos(leftCardModel, -140f);
            SetCardXPos(rightCardModel, 140f);

            //cardGivenSignal.Dispatch(leftCardModel);

            debug.Log(string.Format("Choices setup"));
        }

        private void SetCardXPos(CardModel card, float x)
        {
            var rectTransform = card.rectTransform;
            rectTransform.anchorMax = anchorPosition;
            rectTransform.anchorMin = anchorPosition;
            rectTransform.pivot = anchorPosition;
            rectTransform.anchoredPosition3D = new Vector3(x, 0, 0);
            rectTransform.localScale = Vector3.one;
        }
    }
}

