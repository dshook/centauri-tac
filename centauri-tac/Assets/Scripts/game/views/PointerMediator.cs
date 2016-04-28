using strange.extensions.mediation.impl;
using ctac.signals;

namespace ctac
{
    public class PointerMediator : Mediator
    {
        [Inject]
        public PointerView view { get; set; }

        [Inject]
        public CardSelectedSignal cardStartDrag { get; set; }

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

        public override void OnRegister()
        {
            view.init();

            cardStartDrag.AddListener(onCardDragStart);
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
            cardStartDrag.RemoveListener(onCardDragStart);
            cardActivated.RemoveListener(onCardDragEnd);

            cancelSelectTarget.RemoveListener(onCancelSelectTarget);
            selectTarget.RemoveListener(onSelectTarget);
            startSelectTarget.RemoveListener(onStartTarget);

            cancelSelectAbilityTarget.RemoveListener(onAbilityCancelSelectTarget);
            selectAbilityTarget.RemoveListener(onAbilitySelectTarget);
            startAbilitySelectTarget.RemoveListener(onAbilityStartTarget);
        }

        private void onCardDragStart(CardModel card)
        {
            if (card != null && card.gameObject != null)
            {
                if (card.tags.Contains("Spell"))
                {
                    var targets = possibleActions.GetActionsForCard(turns.currentPlayerId, card.id);
                    //don't point for untargeted spells
                    if (targets == null) { return; }
                }
                view.rectTransform(card.gameObject);
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

        private void onStartTarget(StartTargetModel model)
        {
            if (model.targetingCard != null && model.cardDeployPosition != null)
            {
                view.worldPoint(model.cardDeployPosition.gameObject.transform);
            }
            else if (model.targetingCard.tags.Contains("Spell"))
            {
                var targets = possibleActions.GetActionsForCard(turns.currentPlayerId, model.targetingCard.id);
                //don't point for untargeted spells
                if (targets == null) { return; }

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

        private void onSelectTarget(StartTargetModel card, PieceModel piece)
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

