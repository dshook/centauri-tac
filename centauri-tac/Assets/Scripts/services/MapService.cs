using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace ctac
{
    public interface IMapService
    {
        Dictionary<Vector2, Tile> GetTilesInRadius(Vector2 center, int distance);
        Dictionary<Vector2, Tile> GetMovementTilesInRadius(Vector2 center, int distance);
        int TileDistance(Vector2 a, Vector2 b);
        List<Tile> FindPath(Tile start, Tile end, int maxDist);
        Dictionary<Vector2, Tile> GetNeighbors(Vector2 center);
        Dictionary<Vector2, Tile> GetMovableNeighbors(Vector2 center);
    }

    public class MapService : IMapService
    {
        [Inject]
        public MinionsModel minions { get; set; }

        [Inject]
        public MapModel mapModel { get; set; }

        public Dictionary<Vector2, Tile> GetTilesInRadius(Vector2 center, int distance)
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

                var neighbors = GetNeighbors(current);
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
                        frontier.Add(neighbor.Key);
                    }
                }
            }

            return ret;
        }

        public Dictionary<Vector2, Tile> GetMovementTilesInRadius(Vector2 center, int distance)
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
                frontier.RemoveAt(0);

                if (g_score[current] > distance)
                {
                    continue;
                }

                if (!ret.ContainsKey(current))
                {
                    ret.Add(current, mapModel.tiles[current]);
                }

                var neighbors = GetMovableNeighbors(current);
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

        public List<Tile> FindPath(Tile start, Tile end, int maxDist)
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

                var neighbors = GetMovableNeighbors(current.position);
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
            return total_path;
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

        public Dictionary<Vector2, Tile> GetMovableNeighbors(Vector2 center)
        {
            var ret = GetNeighbors(center);

            return ret
                .Where(t => !minions.minions.Any(x => !x.currentPlayerHasControl && x.tilePosition == t.Key))
                .ToDictionary(k => k.Key, v => v.Value);
        }

    }
}
