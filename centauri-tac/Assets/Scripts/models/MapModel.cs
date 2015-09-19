using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ctac
{
    [Singleton]
    public class MapModel
    {
        public string name { get; set; }
        public int maxPlayers { get; set; }
        public GameObject root { get; set; }
        public Dictionary<Vector2, Tile> tiles { get; set; }

        public Dictionary<Vector2, Tile> GetTilesInRadius(Vector2 center, int distance)
        {
            if (distance <= 0) return new Dictionary<Vector2, Tile>();
            var ret = new Dictionary<Vector2, Tile>();
            var frontier = new List<Vector2>();

            frontier.Add(center);

            while (frontier.Count > 0)
            {
                var current = frontier[0];
                frontier.RemoveAt(0);

                if (!ret.ContainsKey(current))
                {
                    ret.Add(current, tiles[current]);
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

        public List<Tile> FindPath(Tile start, Tile end)
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

                foreach (var neighborDict in GetNeighbors(current.position)) {
                    var neighbor = neighborDict.Value;
                    if(closedset.Contains(neighbor)){
                        continue;
                    }

                    var tentative_g_score = getValueOrMax(g_score,current) + TileDistance(current.position, neighbor.position);

                    if (!openset.Contains(neighbor) || tentative_g_score < getValueOrMax(g_score,neighbor)) {
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
                bool hasKey = tiles.TryGetValue(currentDirection, out next);
                if (hasKey)
                {
                    ret.Add(currentDirection, next);
                }
            }

            return ret;
        }

        public int TileDistance(Vector2 a, Vector2 b)
        {
            return (int)(Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y));
        }

    }

    public class Tile
    {
        public Vector2 position { get; set; }
        public GameObject gameObject { get; set; }
        public TileHighlightStatus highlightStatus;
    }

    [Flags]
    public enum TileHighlightStatus
    {
        None = 0,
        Highlighted = 1,
        Selected = 2,
        Movable = 4,
        PathFind = 8
    }
}

