using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ctac
{
    public interface IMapService
    {
        Dictionary<Vector2, Tile> GetTilesInRadius(Vector2 center, int distance);
        Dictionary<Vector2, Tile> GetKingTilesInRadius(Vector2 center, int distance);
        Dictionary<Vector2, Tile> Expand(List<Vector2> selection, int distance);
        Dictionary<Vector2, Tile> GetMovementTilesInRadius(Vector2 center, int distance, int controllingPlayerId);
        Dictionary<Vector2, Tile> GetLineTiles(Vector2 center, Vector2 secondPoint, int distance, bool bothDirections);
        Dictionary<Vector2, Tile> GetCrossTiles(Vector2 center, int distance);
        int TileDistance(Vector2 a, Vector2 b);
        int KingDistance(Vector2 a, Vector2 b);
        List<Tile> FindPath(Tile start, Tile end, int maxDist, int controllingPlayerId);
        Dictionary<Vector2, Tile> GetNeighbors(Vector2 center);
        Dictionary<Vector2, Tile> GetMovableNeighbors(Tile center, int controllingPlayerId, Tile dest = null);
        bool isHeightPassable(Tile start, Tile end);
        Tile Tile(Vector2 position);
    }

    public class MapService : IMapService
    {
        [Inject]
        public PiecesModel pieces { get; set; }

        [Inject]
        public MapModel mapModel { get; set; }

        public Dictionary<Vector2, Tile> GetKingTilesInRadius(Vector2 center, int distance)
        {
            return GetTilesInRadiusGeneric(center, distance, GetKingNeighbors, KingDistance);
        }

        public Dictionary<Vector2, Tile> GetTilesInRadius(Vector2 center, int distance)
        {
            return GetTilesInRadiusGeneric(center, distance, GetNeighbors, TileDistance);
        }

        private Dictionary<Vector2, Tile> GetTilesInRadiusGeneric(
            Vector2 center, int distance
            , Func<Vector2, Dictionary<Vector2, Tile>> neighborsFunc
            , Func<Vector2, Vector2, int> distanceFunc
        )
        {
            var ret = new Dictionary<Vector2, Tile>();
            if (distance <= 0) return ret;
            var frontier = new List<Vector2>();

            frontier.Add(center);

            while (frontier.Count > 0)
            {
                var current = frontier[0];
                frontier.RemoveAt(0);

                if (!ret.ContainsKey(current))
                {
                    ret.Add(current, mapModel.tiles[current]);
                }

                var neighbors = neighborsFunc(current);
                foreach (var neighbor in neighbors)
                {
                    //add the neighbor to explore if it's not already being returned 
                    //or in the queue or too far away
                    if (
                        !ret.ContainsKey(neighbor.Key) 
                        && !frontier.Contains(neighbor.Key)
                        && distanceFunc(neighbor.Key, center) <= distance
                    )
                    {
                        frontier.Add(neighbor.Key);
                    }
                }
            }

            return ret;
        }

        public Dictionary<Vector2, Tile> GetCrossTiles(Vector2 center, int distance)
        {
            var ret = new Dictionary<Vector2, Tile>()
            {
                { center, mapModel.tiles.Get(center) }
            };
            Tile neighborTile = null;
            for (var d = 1; d <= distance; d++)
            {
                var toCheck = new Vector2[4] { 
                  center.AddX(d),
                  center.AddX(-d),
                  center.AddY(d),
                  center.AddY(-d)
                };

                foreach (var currentDirection in toCheck)
                {
                    //check it's not off the map
                    neighborTile = mapModel.tiles.Get(currentDirection);
                    if (neighborTile != null)
                    {
                        ret[currentDirection] = neighborTile;
                    }
                }
            }
            return ret;
        }

        //uses second point to determine direction of the line, should be within 1 distance of center
        public Dictionary<Vector2, Tile> GetLineTiles(Vector2 center, Vector2 secondPoint, int distance, bool bothDirections)
        {
            var xDiff = secondPoint.x - center.x;
            var zDiff = secondPoint.y - center.y;

            var ret = new Dictionary<Vector2, Tile>()
            {
                { center, mapModel.tiles.Get(center) }
            };
            Tile neighborTile = null;
            for (var d = 1; d <= distance; d++)
            {
                var toCheck = new List<Vector2>(){
                  center.Add(xDiff * d, zDiff * d)
                };
                if (bothDirections)
                {
                    toCheck.Add(center.Add(-xDiff * d, -zDiff * d));
                }

                foreach (var currentDirection in toCheck)
                {
                    //check it's not off the map
                    neighborTile = mapModel.tiles.Get(currentDirection);
                    if (neighborTile != null)
                    {
                        ret[currentDirection] = neighborTile;
                    }
                }
            }
            return ret;
        }

        public Dictionary<Vector2, Tile> Expand(List<Vector2> selection, int distance)
        {
            var ret = new Dictionary<Vector2, Tile>();
            if (distance <= 0) return ret;

            for (int i = 1; i <= distance; i++)
            {
                foreach (var pos in selection)
                {
                    //TODO: optimization for bigger distances would be to remove the 'inner' tiles from the selection
                    //so they aren't rechecked every iteration
                    var neighbors = GetNeighbors(pos);
                    var untouchedNeighbors = neighbors.Where(x => !selection.Contains(x.Key));
                    foreach (var neighbor in untouchedNeighbors)
                    {
                        if (ret.ContainsKey(neighbor.Key)) continue;

                        ret.Add(neighbor.Key, neighbor.Value);
                    }
                }

                selection.ForEach(s => ret.Add(s, mapModel.tiles[s]));
                selection = ret.Keys.ToList();
            }

            return ret;
        }

        public Dictionary<Vector2, Tile> GetMovementTilesInRadius(Vector2 center, int distance, int controllingPlayerId)
        {
            var ret = new Dictionary<Vector2, Tile>();
            if (distance <= 0) return ret;
            var frontier = new List<Vector2>();

            var g_score = new Dictionary<Vector2, int>();
            g_score[center] = 0;    // Cost from start along best known path.

            frontier.Add(center);

            while (frontier.Count > 0)
            {
                var current = frontier[0];
                var currentTile = mapModel.tiles[current];
                frontier.RemoveAt(0);

                if (g_score[current] > distance)
                {
                    continue;
                }

                if (!ret.ContainsKey(current))
                {
                    ret.Add(current, currentTile);
                }

                var neighbors = GetMovableNeighbors(currentTile, controllingPlayerId);
                foreach (var neighbor in neighbors)
                {
                    //add the neighbor to explore if it's not already being returned 
                    //or in the queue or too far away
                    if (
                        !ret.ContainsKey(neighbor.Key) 
                        && !frontier.Contains(neighbor.Key)
                        && TileDistance(neighbor.Key, center) <= distance
                    )
                    {
                        g_score[neighbor.Key] = g_score[current] + 1;
                        frontier.Add(neighbor.Key);
                    }
                }
            }

            return ret;
        }

        public int TileDistance(Vector2 a, Vector2 b)
        {
            return (int)(Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y));
        }

        public int KingDistance(Vector2 a, Vector2 b)
        {
            return Math.Max(
                (int)Mathf.Abs(a.x - b.x),
                (int)Mathf.Abs(a.y - b.y)
            );
        }

        public List<Tile> FindPath(Tile start, Tile end, int maxDist, int controllingPlayerId)
        {
            var ret = new List<Tile>();
            if(start == end) return ret;

            // The set of nodes already evaluated.
            var closedset = new List<Tile>();

            // The set of tentative nodes to be evaluated, initially containing the start node
            var openset = new List<Tile>(){ start };

            // The map of navigated nodes.
            var came_from = new Dictionary<Tile, Tile>();

            var g_score = new Dictionary<Tile, int>();
            g_score[start] = 0;    // Cost from start along best known path.

            // Estimated total cost from start to goal through y.
            var f_score = new Dictionary<Tile, int>();
            f_score[start] = g_score[start] + heuristic_cost_estimate(start, end);

            while (openset.Count > 0) {
                // the node in openset having the lowest f_score[] value
                var current = openset.OrderBy(x => getValueOrMax(f_score,x)).First();
                if (current == end) {
                    return ReconstructPath(came_from, end);
                }

                openset.Remove(current);
                closedset.Add(current);

                var neighbors = GetMovableNeighbors(current, controllingPlayerId, end);
                foreach (var neighborDict in neighbors) {
                    var neighbor = neighborDict.Value;
                    if(closedset.Contains(neighbor)){
                        continue;
                    }

                    var tentative_g_score = getValueOrMax(g_score,current) + TileDistance(current.position, neighbor.position);


                    if (!openset.Contains(neighbor) || tentative_g_score < getValueOrMax(g_score,neighbor)) {
                        //check for max dist along path
                        if (tentative_g_score > maxDist)
                        {
                            continue;
                        }

                        came_from[neighbor] = current;
                        g_score[neighbor] = tentative_g_score;
                        f_score[neighbor] = getValueOrMax(g_score,neighbor) + TileDistance(neighbor.position, end.position);
                        if (!openset.Contains(neighbor)) {
                            openset.Add(neighbor);
                        }
                    }
                }

            }

            return null;
        }

        private int getValueOrMax(Dictionary<Tile, int> dict, Tile key)
        {
            if(dict.ContainsKey(key)) return dict[key];
            return int.MaxValue;
        }

        private int heuristic_cost_estimate(Tile start, Tile end)
        {
            return TileDistance(start.position, end.position);
        }

        private List<Tile> ReconstructPath(Dictionary<Tile, Tile> came_from, Tile current) {
            var total_path = new List<Tile>() { current };
            while( came_from.ContainsKey(current)){
                current = came_from[current];
                total_path.Add(current);
            }
            total_path.Reverse();
            //remove starting tile
            return total_path.Skip(1).ToList();
        }

        public Dictionary<Vector2, Tile> GetNeighbors(Vector2 center)
        {
            var ret = new Dictionary<Vector2, Tile>();
            Tile next = null;
            var toCheck = new Vector2[4]{
                center.AddX(1f),
                center.AddX(-1f),
                center.AddY(1f),
                center.AddY(-1f)
            };

            foreach (var currentDirection in toCheck)
            {
                //check it's not off the map
                next = mapModel.tiles.Get(currentDirection);
                if (next != null)
                {
                    ret.Add(currentDirection, next);
                }
            }
            return ret;
        }

        public Dictionary<Vector2, Tile> GetKingNeighbors(Vector2 center)
        {
            var ret = new Dictionary<Vector2, Tile>();
            Tile next = null;
            var toCheck = new Vector2[8]{
                center.AddX(1f),
                center.AddX(-1f),
                center.AddY(1f),
                center.AddY(-1f),
                center.Add(-1, -1),
                center.Add(-1, 1),
                center.Add(1, -1),
                center.Add(1, 1)
            };

            foreach (var currentDirection in toCheck)
            {
                //check it's not off the map
                next = mapModel.tiles.Get(currentDirection);
                if (next != null)
                {
                    ret.Add(currentDirection, next);
                }
            }
            return ret;
        }

        public bool isHeightPassable(Tile start, Tile end)
        {
            return Math.Abs(start.fullPosition.y - end.fullPosition.y ) < Constants.heightDeltaThreshold;
        }

        public Tile Tile(Vector2 position)
        {
            return mapModel.tiles[position];
        }

        /// <summary>
        /// Find neighboring tiles that aren't occupied by enemies,
        /// but always include the dest tile for attacking if it's passed
        /// but also make sure not to land on a tile with an occupant if attacking
        /// </summary>
        public Dictionary<Vector2, Tile> GetMovableNeighbors(Tile center, int controllingPlayerId, Tile dest = null)
        {
            var ret = GetNeighbors(center.position);

            //filter tiles that are too high/low to move to
            ret = ret.Where(t => isHeightPassable(t.Value, center)).ToDictionary(k => k.Key, v => v.Value);

            //filter out tiles with enemies on them that aren't the destination
            ret = ret.Where(t => 
                (dest != null && t.Key == dest.position) ||
                !pieces.Pieces.Any(m => m.tilePosition == t.Key && m.playerId != controllingPlayerId)
            ).ToDictionary(k => k.Key, v => v.Value);

            bool destinationOccupied = dest != null && pieces.Pieces.Any(p => p.tilePosition == dest.position);
            
            //make sure not to consider tiles that would be where the moving pieces lands when it attacks
            ret = ret.Where(t => 
                dest == null
                || dest.position == t.Key
                || !destinationOccupied
                || TileDistance(t.Key, dest.position) > 1
                || !pieces.Pieces.Any(p => p.tilePosition == t.Key)
            ).ToDictionary(k => k.Key, v => v.Value);

            return ret;
        }

    }
}

