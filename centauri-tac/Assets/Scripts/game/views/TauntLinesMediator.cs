using strange.extensions.mediation.impl;
using ctac.signals;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ctac
{
    public class TauntLinesMediator : Mediator
    {
        [Inject] public TauntLinesView view { get; set; }
        [Inject] public TauntTilesUpdatedSignal tauntUpdated { get; set; }

        [Inject] public MapModel map { get; set; }

        public override void OnRegister()
        {
            tauntUpdated.AddListener(onTilesUpdate);
            view.init();
        }

        public override void onRemove()
        {
            tauntUpdated.RemoveListener(onTilesUpdate);
        }

        private void onTilesUpdate(TauntTilesUpdateModel tilesUpdated)
        {
            var friendlyPerims = FindTilePerimeters(tilesUpdated.friendlyTauntTiles);
            var enemyPerims    = FindTilePerimeters(tilesUpdated.enemyTauntTiles);

            view.UpdatePerims(friendlyPerims, map);
        }

        //given a list of taunt tiles, find any perimeter loops which there could be multiple of
        private List<List<Tile>> FindTilePerimeters(List<Tile> tiles)
        {
            if(tiles == null || tiles.Count == 0) return null;
            //start by finding the top right most tile to start a loop from (from a certain perspective it's top right at least)
            var startTile = tiles.OrderByDescending(t => t.position.x).ThenByDescending(t => t.position.y).FirstOrDefault();
            var loops = new List<List<Tile>>();

            var firstLoop = FindPerimeterLoop(startTile, tiles);
            loops.Add(firstLoop);

            float minX, maxX, minY, maxY;
            minX = firstLoop.Min(t => t.position.x);
            maxX = firstLoop.Max(t => t.position.x);
            minY = firstLoop.Min(t => t.position.y);
            maxY = firstLoop.Max(t => t.position.y);

            //var remainingTiles = tiles.Where(t => t.position.x );
            //once that first loop is complete, search for any tiles strictly outside of that loop
            return loops;
        }

        //in a list of tiles, find the perim loop, assuming the taunt minons will always have the group of 9 tiles
        private List<Tile> FindPerimeterLoop(Tile startTile, List<Tile> tiles)
        {
            //trace right and down as much as we can until the bottom is found, then start going left and up
            //but make sure not to trace back onto ourselves or into the middle
            var perim = new List<Tile>();
            var tileLookup = tiles.ToDictionary(k => k.position, v => v);
            var travelDirection = TTDir.Down;
            var currentTile = startTile;
            do
            {
                var directionPriorities = directionPriority(travelDirection);
                foreach (var direction in directionPriorities)
                {
                    var nextTilePos = MoveVec2(currentTile.position, direction);
                    if (tileLookup.ContainsKey(nextTilePos))
                    {
                        perim.Add(currentTile);
                        travelDirection = direction;
                        currentTile = tileLookup[nextTilePos];
                        break;
                    }
                }
            }
            while (currentTile != startTile);

            return perim;
        }

        //Return the list of all tiles in the perimeter and whatever is inside of the perimeter
        //this uses a scan line approach starting from the top right as before and scanning left
        private List<Tile> FloodFillPerimeterLoop(List<Tile> fullTileList, List<Tile> perimeter)
        {
            var startTile = perimeter.OrderByDescending(t => t.position.y).ThenByDescending(t => t.position.x).FirstOrDefault();
            var tileLookup = fullTileList.ToDictionary(k => k.position, v => v);
            var perimeterLookup = perimeter.ToDictionary(k => k.position, v => v);

            var retList = new List<Tile>();
            float minX, maxX, minY, maxY;
            minY = perimeter.Min(t => t.position.y);
            maxY = perimeter.Max(t => t.position.y);
            minX = perimeter.Min(t => t.position.x);
            maxX = perimeter.Max(t => t.position.x);

            var listToCheck = new List<Tile>();
            listToCheck.AddRange(perimeter);


            return retList;
        }

        private static TTDir[] rightPriorities =  { TTDir.Right, TTDir.Down, TTDir.Up };
        private static TTDir[] downPriorities =  { TTDir.Right, TTDir.Down, TTDir.Left };
        private static TTDir[] leftPriorities =  { TTDir.Down, TTDir.Left, TTDir.Up };
        private static TTDir[] upPriorities =  { TTDir.Left, TTDir.Up, TTDir.Right };
        //given a direction we're going, which ways should we go next
        private TTDir[] directionPriority(TTDir dir)
        {
            switch (dir) {
                case TTDir.Right:
                    return rightPriorities;
                case TTDir.Down:
                    return downPriorities;
                case TTDir.Left:
                    return leftPriorities;
                case TTDir.Up:
                    return upPriorities;
            }

            return null;
        }

        private Vector2 MoveVec2(Vector2 cur, TTDir dir)
        {
            switch (dir) {
                case TTDir.Right:
                    return cur.AddX(1);
                case TTDir.Down:
                    return cur.AddY(-1);
                case TTDir.Left:
                    return cur.AddX(-1);
                case TTDir.Up:
                    return cur.AddY(1);
            }
            return cur;
        }
    }
    public enum TTDir
    {
        Right = 0,
        Down = 1,
        Left = 2,
        Up = 3
    }
}

