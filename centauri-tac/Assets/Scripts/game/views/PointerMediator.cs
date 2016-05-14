using strange.extensions.mediation.impl;
using ctac.signals;

namespace ctac
{
    public class PointerMediator : Mediator
    {
        [Inject]
        public PointerView view { get; set; }

        [Inject]
        public CardSelectedSignal cardSelected { get; set; }

        [Inject]
        public ActivateCardSignal cardActivated { get; set; }

        [Inject] public StartSelectTargetSignal startSelectTarget { get; set; }
        [Inject] public SelectTargetSignal selectTarget { get; set; }
        [Inject] public CancelSelectTargetSignal cancelSelectTarget { get; set; }

        [Inject] public StartSelectAbilityTargetSignal startAbilitySelectTarget { get; set; }
        [Inject] public SelectAbilityTargetSignal selectAbilityTarget { get; set; }
        [Inject] public CancelSelectAbilityTargetSignal cancelSelectAbilityTarget { get; set; }

        [Inject] public PossibleActionsModel possibleActions { get; set; }
        [Inject] public GameTurnModel turns { get; set; }
        [Inject] public RaycastModel raycastModel { get; set; }

        public override void OnRegister()
        {
            view.init(raycastModel);

            cardSelected.AddListener(onCardSelected);
            cardActivated.AddListener(onCardDragEnd);

            cancelSelectTarget.AddListener(onCancelSelectTarget);
            selectTarget.AddListener(onSelectTarget);
            startSelectTarget.AddListener(onStartTarget);

            cancelSelectAbilityTarget.AddListener(onAbilityCancelSelectTarget);
            selectAbilityTarget.AddListener(onAbilitySelectTarget);
            startAbilitySelectTarget.AddListener(onAbilityStartTarget);
        }

        public override void onRemove()
        {
            cardSelected.RemoveListener(onCardSelected);
            cardActivated.RemoveListener(onCardDragEnd);

            cancelSelectTarget.RemoveListener(onCancelSelectTarget);
            selectTarget.RemoveListener(onSelectTarget);
            startSelectTarget.RemoveListener(onStartTarget);

            cancelSelectAbilityTarget.RemoveListener(onAbilityCancelSelectTarget);
            selectAbilityTarget.RemoveListener(onAbilitySelectTarget);
            startAbilitySelectTarget.RemoveListener(onAbilityStartTarget);
        }

        private void onCardSelected(CardSelectedModel cardModel)
        {
            if (cardModel != null && cardModel.card.gameObject != null)
            {
                //don't point for untargeted or un aread spells
                if (cardModel.card.isSpell && !cardModel.card.needsTargeting(possibleActions))
                {
                    return;
                }
                view.rectTransform(cardModel.card.gameObject);
            }
            else
            {
                view.disable();
            }
        }

        private void onCardDragEnd(ActivateModel m)
        {
            view.disable();
        }

        private void onStartTarget(TargetModel model)
        {
            if (model.targetingCard != null && model.cardDeployPosition != null)
            {
                view.worldPoint(model.cardDeployPosition.gameObject.transform);
            }
            else if (model.targetingCard.isSpell)
            {
                var area = possibleActions.GetAreasForCard(turns.currentPlayerId, model.targetingCard.id);
                //don't point for untargeted spells
                if (model.targets == null && area == null) { return; }

                view.rectTransform(model.targetingCard.gameObject);
            }
            else
            {
                view.disable();
            }
        }

        private void onCancelSelectTarget(CardModel card)
        {
            view.disable();
        }

        private void onSelectTarget(TargetModel card)
        {
            view.disable();
        }

        private void onAbilityStartTarget(StartAbilityTargetModel model)
        {
            if (model.targetingPiece != null && model.targetingPiece.gameObject != null)
            {
                view.worldPoint(model.targetingPiece.gameObject.transform);
            }
            else
            {
                view.disable();
            }
        }

        private void onAbilityCancelSelectTarget(PieceModel card)
        {
            view.disable();
        }

        private void onAbilitySelectTarget(StartAbilityTargetModel card, PieceModel piece)
        {
            view.disable();
        }

    }
}

