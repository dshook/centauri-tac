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

        [Inject]
        public StartSelectTargetSignal startSelectTarget { get; set; }

        [Inject]
        public SelectTargetSignal selectTarget { get; set; }

        [Inject]
        public CancelSelectTargetSignal cancelSelectTarget { get; set; }

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
        }

        public override void onRemove()
        {
            cardStartDrag.RemoveListener(onCardDragStart);
            cardActivated.RemoveListener(onCardDragEnd);
            cancelSelectTarget.RemoveListener(onCancelSelectTarget);
            selectTarget.RemoveListener(onSelectTarget);
            startSelectTarget.RemoveListener(onStartTarget);
        }

        private void onCardDragStart(CardModel card)
        {
            if (card != null && card.gameObject != null)
            {
                if (card.tags.Contains("Spell"))
                {
                    var targets = possibleActions.GetForCard(turns.currentPlayerId, card.id);
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

        private void onStartTarget(CardModel card, Tile where, ActionTarget at)
        {
            if (card != null && card.gameObject != null)
            {
                view.rectTransform(card.gameObject);
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

        private void onSelectTarget(CardModel card, PieceModel piece)
        {
            view.disable();
        }

    }
}

