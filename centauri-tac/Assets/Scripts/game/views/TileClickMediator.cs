using UnityEngine;
using strange.extensions.mediation.impl;
using ctac.signals;

namespace ctac
{
    public class TileClickMediator : Mediator
    {
        [Inject]
        public TileClickView view { get; set; }

        [Inject]
        public MinionSelectedSignal minionSelected { get; set; }

        [Inject]
        public AttackMinionSignal attackMinion { get; set; }

        [Inject]
        public MoveMinionSignal moveMinion { get; set; }

        [Inject]
        public MapModel map { get; set; }

        public override void OnRegister()
        {
            minionSelected.AddListener(onMinionSelected);
            view.clickSignal.AddListener(onClick);
            view.init();
        }

        private MinionModel selectedMinion = null;

        public override void onRemove()
        {
            minionSelected.RemoveListener(onMinionSelected);
        }

        private void onClick(GameObject clickedObject)
        {
            if (clickedObject != null)
            {
                if (clickedObject.CompareTag("Minion"))
                {
                    var minionView = clickedObject.GetComponent<MinionView>();
                    if (minionView.minion.currentPlayerHasControl)
                    {
                        minionSelected.Dispatch(minionView.minion);
                    }
                    else
                    {
                        if (selectedMinion != null)
                        {
                            attackMinion.Dispatch(new AttackPieceModel()
                            {
                                attackingPieceId = selectedMinion.id,
                                targetPieceId = minionView.minion.id
                            });
                            minionSelected.Dispatch(null);
                        }
                    }
                    return;
                }

                if (clickedObject.CompareTag("Tile"))
                {
                    var gameTile = map.tiles.Get(clickedObject.transform.position.ToTileCoordinates());

                    if (FlagsHelper.IsSet(gameTile.highlightStatus, TileHighlightStatus.Movable) && selectedMinion != null)
                    {
                        moveMinion.Dispatch(selectedMinion, gameTile);
                        minionSelected.Dispatch(null);
                    }
                }
            }
            else
            {
                minionSelected.Dispatch(null);
            }

        }

        private void onMinionSelected(MinionModel minionSelected)
        {
            selectedMinion = minionSelected;
        }
    }
}

