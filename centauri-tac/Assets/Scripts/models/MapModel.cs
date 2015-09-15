using System.Collections.Generic;
using UnityEngine;

namespace ctac
{
    public interface IMapModel
    {
        string name { get; set; }
        int maxPlayers { get; set; }
        GameObject root { get; set; }
        Dictionary<Vector2, Tile> tiles { get; set; }

        Dictionary<Vector2, Tile> GetTilesInRadius(Vector2 center, int distance);
        Dictionary<Vector2, Tile> GetNeighbors(Vector2 center);
    }

    [Singleton]
    public class MapModel : IMapModel
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
        public GameObject gameObject { get; set; }
    }
}

