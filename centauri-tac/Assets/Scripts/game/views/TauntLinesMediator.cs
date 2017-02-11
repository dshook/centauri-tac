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
        [Inject] public IMapService mapService { get; set; }

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
            var friendlyPerims = FindTilePerimeters(tilesUpdated.friendlyTauntTiles, tilesUpdated.friendlyTauntPieceTiles);
            var enemyPerims    = FindTilePerimeters(tilesUpdated.enemyTauntTiles, tilesUpdated.enemyTauntPieceTiles);

            view.ResetPerims();
            view.UpdatePerims(friendlyPerims, map, true);
            view.UpdatePerims(enemyPerims, map, false);
        }

        //given a list of taunt tiles, find any perimeter loops which there could be multiple of
        private List<List<Tile>> FindTilePerimeters(List<Tile> tiles, List<Tile> pieceTiles)
        {
            if(tiles == null || tiles.Count == 0) return null;

            var tauntGroups = FindTauntGroups(pieceTiles);
            var loops = new List<List<Tile>>();

            foreach (var tauntGroup in tauntGroups)
            {
                //start by finding the top right most tile to start a loop from (from a certain perspective it's top right at least)
                var startTile = tauntGroup.OrderByDescending(t => t.position.x).ThenByDescending(t => t.position.y).FirstOrDefault();

                var perimLoop = FindPerimeterLoop(startTile, tauntGroup);
                loops.Add(perimLoop);
            }

            return loops;
        }

        //group up the surrounding tiles around the pieces that are taunting into groups that represent the whole taunt area
        private List<List<Tile>> FindTauntGroups(List<Tile> pieceTiles)
        {
            var tauntGroups = new List<List<Tile>>();

            for (int i = 0; i < pieceTiles.Count; i++)
            {
                var tilePosition = pieceTiles[i].position;
                var kingTiles = mapService.GetKingTilesInRadius(tilePosition, 1).Select(k => k.Value).ToList();
                kingTiles = kingTiles.Where(t =>
                    mapService.isHeightPassable(t, mapService.Tile(tilePosition))).ToList();

                var intGroups = FindIntersectingGroups(tauntGroups, kingTiles);

                if (intGroups.Count == 0)
                {
                    tauntGroups.Add(kingTiles);
                }
                else if (intGroups.Count == 1)
                {
                    tauntGroups[intGroups[0]].AddRange(kingTiles);
                }
                else
                {
                    var mergedGroup = new List<Tile>();
                    //merge groups if more than one intersected
                    for (int tg = intGroups.Count; tg >= 0; tg--)
                    {
                        var tgIndex = intGroups[tg];
                        mergedGroup.AddRange(tauntGroups[tgIndex]);
                        tauntGroups.RemoveAt(tgIndex);
                    }
                }
            }

            //Make sure to take tile duplicates out
            return tauntGroups.Select(tg => tg.Distinct().ToList()).ToList();
        }

        //returns the indexes in the tile groups for the intersecting new piece tiles
        private List<int> FindIntersectingGroups(List<List<Tile>> tileGroups, List<Tile> newPieceTiles)
        {
            var intGroups = new List<int>();
            for (int i = 0; i < tileGroups.Count; i++)
            {
                var tilesInGroup = tileGroups[i];
                for (int pt = 0; pt < newPieceTiles.Count; pt++) {
                    if (tilesInGroup.Any(t => mapService.TileDistance(t.position, newPieceTiles[pt].position) <= 1))
                    {
                        intGroups.Add(i);
                        break;
                    }
                }
            }
            return intGroups;
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

        private static TTDir[] rightPriorities =  { TTDir.Up, TTDir.Right, TTDir.Down};
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

