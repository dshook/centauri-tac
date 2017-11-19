using strange.extensions.mediation.impl;
using ctac.signals;

namespace ctac
{
    public class PointerMediator : Mediator
    {
        [Inject]
        public PointerView view { get; set; }

        [Inject] public PossibleActionsModel possibleActions { get; set; }
        [Inject] public GamePlayersModel players { get; set; }
        [Inject] public RaycastModel raycastModel { get; set; }

        [Inject] public IDebugService debug { get; set; }

        bool targeting = false;

        public override void OnRegister()
        {
            view.init(raycastModel);
        }

        //[ListensTo(typeof(CardSelectedSignal))]
        //public void onCardSelected(CardSelectedModel cardModel)
        //{
        //    if (cardModel != null && cardModel.card.gameObject != null)
        //    {
        //        //don't point for untargeted or un aread spells
        //        if (cardModel.card.isSpell && !cardModel.card.needsTargeting(possibleActions))
        //        {
        //            return;
        //        }
        //        view.rectTransform(cardModel.card.gameObject);
        //        debug.Log("Enabling pointer from selected card");
        //    }
        //    else if(!targeting)
        //    {
        //        view.disable();
        //        debug.Log("Disabling pointer from selected card");
        //    }
        //}

        [ListensTo(typeof(PieceSpawningSignal))]
        public void onPieceSpawning(CardSelectedModel cardModel)
        {
            if (cardModel != null)
            {
                view.rectTransform(cardModel.card.gameObject);
                debug.Log("Enabling pointer from deploying piece");
            }
            else
            {
                view.disable();
                debug.Log("Disabling pointer for piece spawning");
            }
        }

        [ListensTo(typeof(ActivateCardSignal))]
        public void onCardActivated(ActivateModel m)
        {
            view.disable();
        }

        [ListensTo(typeof(StartSelectTargetSignal))]
        public void onStartTarget(TargetModel model)
        {
            if (model.targetingCard != null && model.cardDeployPosition != null)
            {
                view.worldPoint(model.cardDeployPosition.gameObject.transform);
                targeting = true;
            }
            else if (model.targetingCard.isSpell)
            {
                var area = possibleActions.GetAreasForCard(players.Me.id, model.targetingCard.id);
                //don't point for untargeted spells
                if (model.targets == null && area == null) { return; }

                view.rectTransform(model.targetingCard.gameObject);
                targeting = true;
            }
            else
            {
                view.disable();
                targeting = false;
            }
        }

        [ListensTo(typeof(CancelSelectTargetSignal))]
        public void onCancelSelectTarget(CardModel card)
        {
            view.disable();
            targeting = false;
        }

        [ListensTo(typeof(SelectTargetSignal))]
        public void onSelectTarget(TargetModel card)
        {
            view.disable();
            targeting = false;
        }

        [ListensTo(typeof(StartSelectAbilityTargetSignal))]
        public void onAbilityStartTarget(StartAbilityTargetModel model)
        {
            if (model.targetingPiece != null && model.targetingPiece.gameObject != null)
            {
                view.worldPoint(model.targetingPiece.gameObject.transform);
                targeting = true;
            }
            else
            {
                view.disable();
                targeting = false;
            }
        }

        [ListensTo(typeof(CancelSelectAbilityTargetSignal))]
        public void onAbilityCancelSelectTarget(PieceModel card)
        {
            view.disable();
            targeting = false;
        }

        [ListensTo(typeof(SelectAbilityTargetSignal))]
        public void onAbilitySelectTarget(StartAbilityTargetModel card, PieceModel piece)
        {
            view.disable();
            targeting = false;
        }

        [ListensTo(typeof(StartChooseSignal))]
        public void onStartChoose(ChooseModel c)
        {
            view.disable();
            targeting = false;
        }

    }
}

