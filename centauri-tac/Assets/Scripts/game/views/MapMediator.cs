using strange.extensions.mediation.impl;
using ctac.signals;
using System.Linq;
using UnityEngine;

namespace ctac
{
    public class MapMediator : Mediator
    {
        [Inject] public TilesClearedSignal tilesCleared { get; set; }

        [Inject] public AnimationQueueModel animationQueue { get; set; }

        [Inject] public IMapService mapService { get; set; }
        [Inject] public IResourceLoaderService loader { get; set; }

        public override void OnRegister()
        {
            tilesCleared.AddListener(onTilesCleared);
        }

        public override void onRemove()
        {
            tilesCleared.RemoveListener(onTilesCleared);
        }

        public void onTilesCleared(TilesClearedModel tilesModel)
        {
            //animationQueue.Add(
            //    new PieceView.RotateAnim()
            //    {
            //        piece = view,
            //        destAngle = DirectionAngle.angle[tilesModel.direction]
            //    }
            //);
            Debug.Log("Made it to tile cleared mediator!");

        }
    }
}

