using strange.extensions.mediation.impl;
using ctac.signals;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

namespace ctac
{
    public class MapMediator : Mediator
    {
        [Inject] public AnimationQueueModel animationQueue { get; set; }

        [Inject] public MapModel map { get; set; }
        [Inject] public IResourceLoaderService loader { get; set; }

        private GameObject propDestroyParticle = null;

        public override void OnRegister()
        {
            propDestroyParticle = loader.Load<GameObject>("particles/Smoke Explosion");
        }

        [ListensTo(typeof(TilesClearedSignal))]
        public void onTilesCleared(TilesClearedModel tilesModel)
        {
            List<PropView> allToRemove = new List<PropView>();
            foreach (var tilePosition in tilesModel.tilePositions) {
                if (!map.tiles.ContainsKey(tilePosition.Vector2))
                {
                    continue;
                }
                var tile = map.tiles[tilePosition.Vector2];
                var tPos = tile.fullPosition;

                //find all props that are breakable on top of the tile. 
                //Tiles are positioned from center so we have to do this 0.5 juggling
                var propsToDestroy = map.props.Where(p => 
                    p.breakable && 
                    p.transform.position.x >= tPos.x - 0.5f &&
                    p.transform.position.x <= tPos.x + 0.5f &&
                    p.transform.position.z >= tPos.z - 0.5f &&
                    p.transform.position.z <= tPos.z + 0.5f 
                );

                allToRemove.AddRange(propsToDestroy);

                if (propsToDestroy.Count() > 0)
                {
                    animationQueue.Add(
                        new MapView.ClearPropAnim()
                        {
                            particleEffectPrefab = propDestroyParticle,
                            particleLocation = tile.fullPosition,
                            propsToDestroy = propsToDestroy.Select(p => p.gameObject).ToList()
                        }
                    );
                }
            }

            foreach (var propToDestroy in allToRemove)
            {
                map.props.Remove(propToDestroy);
            }
            Debug.Log("Made it to tile cleared mediator!");

        }
    }
}

