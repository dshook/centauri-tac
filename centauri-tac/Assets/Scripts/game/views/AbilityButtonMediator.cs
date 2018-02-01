using strange.extensions.mediation.impl;
using ctac.signals;

namespace ctac
{
    public class AbilityButtonMediator : Mediator
    {
        [Inject]
        public AbilityButtonView view { get; set; }

        [Inject] public ActivateAbilitySignal activateAbility { get; set; }

        [Inject] public PieceHoverSignal pieceHovered { get; set; }

        [Inject] public StartSelectAbilityTargetSignal startSelectTarget { get; set; }

        [Inject] public PiecesModel pieces { get; set; }
        [Inject] public GamePlayersModel players { get; set; }
        [Inject] public PlayerResourcesModel playerResources { get; set; }
        [Inject] public PossibleActionsModel possibleActions { get; set; }

        //for ability targeting
        private StartAbilityTargetModel startTargetModel;

        public override void OnRegister()
        {
            view.clickSignal.AddListener(onAbilityClicked);
            view.hoverSignal.AddListener(onAbilityHover);

            if (view.ability != null)
            {
                var piece = pieces.Piece(view.ability.pieceId);
                view.init(playerResources, piece, players);
            }
        }

        public override void OnRemove()
        {
            view.clickSignal.RemoveListener(onAbilityClicked);
        }

        private void onAbilityClicked()
        {
            var piece = view.piece;
            var targets = possibleActions.GetAbilitiesForPiece(piece.playerId, piece.id);
            if (targets != null && targets.targetPieceIds.Count >= 1)
            {
                //record state we need to maintain for subsequent clicks then dispatch the start target
                startTargetModel = new StartAbilityTargetModel()
                {
                    targetingPiece = piece,
                    targets = targets
                };

                //delay sending off the start select target signal till the card deselected event has cleared
                Invoke("StartSelectTargets", 0.10f);
            }
            else
            {
                activateAbility.Dispatch(new ActivateAbilityModel()
                {
                    piece = piece,
                    optionalTarget = null
                });
            }
        }

        private void onAbilityHover(bool hover)
        {
            if (hover)
            {
                pieceHovered.Dispatch(view.piece);
            }
            else
            {
                pieceHovered.Dispatch(null);
            }
        }

        private void StartSelectTargets()
        {
            startSelectTarget.Dispatch(startTargetModel);
        }

        [ListensTo(typeof(CancelSelectAbilityTargetSignal))]
        public void onTargetCancel(PieceModel card)
        {
            startTargetModel = null;
        }

        [ListensTo(typeof(SelectAbilityTargetSignal))]
        public void onSelectedTarget(StartAbilityTargetModel targetModel, PieceModel piece)
        {
            if(targetModel.targetingPiece.id != view.ability.pieceId) return;

            activateAbility.Dispatch(new ActivateAbilityModel()
            {
                piece = targetModel.targetingPiece,
                optionalTarget = piece
            });
            startTargetModel = null;
        }

    }
}

