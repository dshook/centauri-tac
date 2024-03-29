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
        Dictionary<Vector2, Tile> GetDiagonalTilesInRadius(Vector2 center, int distance);
        Dictionary<Vector2, Tile> Expand(List<Vector2> selection, int distance);
        Dictionary<Vector2, Tile> GetMovementTilesInRadius(PieceModel piece, bool totalMovement, bool includeOccupied = false, int bonusMovement = 0);
        Dictionary<Vector2, Tile> GetLineTiles(Vector2 center, Vector2 secondPoint, int distance, bool bothDirections);
        Dictionary<Vector2, Tile> GetCrossTiles(Vector2 center, int distance);
        int TileDistance(Vector2 a, Vector2 b);
        int KingDistance(Vector2 a, Vector2 b);
        List<Tile> FindMovePath(PieceModel piece, PieceModel attackingPiece, Tile end);
        Dictionary<Vector2, Tile> GetNeighbors(Vector2 center);
        Dictionary<Vector2, Tile> GetMovableNeighbors(Tile center, PieceModel piece, Tile dest, bool includeOccupied);
        List<Tile> CleavePositions(Vector2 position, Direction direction);
        List<Tile> PiercePositions(Vector2 position, Direction direction);
        bool isHeightPassable(Tile start, Tile end);
        Tile Tile(Vector2 position);
        Direction FaceDirection(Vector2 start, Vector2 end);
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

        public Dictionary<Vector2, Tile> GetDiagonalTilesInRadius(Vector2 center, int distance)
        {
            return GetTilesInRadiusGeneric(center, distance, GetDiagonalNeighbors, TileDistance);
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

        /// <summary>
        /// Find all the tiles a piece can move to, if totalMovement is true this will be based on their base movement
        /// Otherwise, it'll be based on how many tiles left they can move
        /// Include occupied signals whether or not to return tiles that are blocked by minions. With it set to true
        /// this will include tiles the piece could move to on an empty board
        /// </summary>
        public Dictionary<Vector2, Tile> GetMovementTilesInRadius(PieceModel piece, bool totalMovement, bool includeOccupied = false, int bonusMovement = 0)
        {
            if (!piece.canMove && !totalMovement && bonusMovement == 0)
            {
                return null;
            }

            var movementRange = (totalMovement ? piece.movement : piece.movement - piece.moveCount) + bonusMovement;
            if ((piece.statuses & Statuses.Flying) != 0)
            {
                var tiles = GetTilesInRadius(piece.tilePosition, movementRange);
                //filter out all the unsuitable tile positions
                return tiles.Where(t =>
                    !t.Value.unpassable
                    && (includeOccupied || !pieces.Pieces.Any(m => m.tilePosition == t.Key))
                ).ToDictionary(k => k.Key, v => v.Value);
            }
            var ret = GetMovementTilesInRadius(piece.tilePosition, movementRange, piece, includeOccupied);
            //filter out friendly pieces from the mix finally. These are still in up until now since you can pass through them
            if(!includeOccupied){
                ret = ret.Where(t => !pieces.Pieces.Any(m => m.tilePosition == t.Key)).ToDictionary(k => k.Key, v => v.Value);
            }
            return ret;
        }

        private Dictionary<Vector2, Tile> GetMovementTilesInRadius(Vector2 center, int distance, PieceModel piece, bool includeOccupied)
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

                var neighbors = GetMovableNeighbors(currentTile, piece, null, includeOccupied);
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

        /// <summary>
        /// Find a path from piece to either the attacking piece or end tile
        /// If attacking piece is passed the path will include the tile they're standing on
        /// </summary>
        public List<Tile> FindMovePath(PieceModel piece, PieceModel pieceAttacking, Tile end)
        {
            if (piece == null || (end != null && end.unpassable) || (pieceAttacking == null && end == null))
            {
                return null;
            }

            //use pieceAttacking if you want to attack a piece on a tile
            if (end != null && pieceAttacking == null && pieces.Pieces.Any(m => m.tilePosition == end.position))
            {
                return null;
            }

            Tile pieceAttackingTile = null;
            if(pieceAttacking != null){
                pieceAttackingTile = mapModel.tiles[pieceAttacking.tilePosition];
            }

            //For ranged units attacking within their range the move path is just the enemy position
            if(
                piece.isRanged
                && pieceAttacking != null
                && KingDistance(piece.tilePosition, pieceAttacking.tilePosition) <= piece.range
            )
            {
                return new List<Tile>() { pieceAttackingTile };
            }

            //Flying pieces can be a bit trickier
            if ((piece.statuses & Statuses.Flying) != 0)
            {
                if (pieceAttacking != null)
                {
                    //for attacking a piece with a flying unit we just need to get to an adjacent open tile that's within range
                    var adjacent = GetMovableNeighbors(pieceAttackingTile, piece, null, false);
                    if (adjacent == null || adjacent.Count == 0)
                    {
                        return null;
                    }
                    var moveTo = adjacent.OrderBy(k => TileDistance(piece.tilePosition, k.Key)).FirstOrDefault();
                    //don't need originating if they're already adjacent though
                    if (moveTo.Value.position == piece.tilePosition)
                    {
                        return new List<Tile>() { pieceAttackingTile };
                    }
                    else
                    {
                        return new List<Tile>() { moveTo.Value, pieceAttackingTile};
                    }
                }
                else if (end != null && TileDistance(piece.tilePosition, end.position) <= piece.movement - piece.moveCount)
                {
                    return new List<Tile>() { end };
                }
            }

            //default cases where the piece is melee or a ranged unit that's just moving to a tile
            //add an extra tile of movement if the destination is an enemy to attack since you don't have to go all the way to them
            var boost = pieceAttacking != null ? 1 : 0;
            var dest = pieceAttacking != null ? pieceAttackingTile : end;
            return FindPath(mapModel.tiles[piece.tilePosition], dest, (piece.movement - piece.moveCount) + boost, piece);
        }

        private List<Tile> FindPath(Tile start, Tile end, int maxDist, PieceModel piece)
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

                var neighbors = GetMovableNeighbors(current, piece, end, false);
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
            var toCheck = new Vector2[4]{
                center.AddX(1f),
                center.AddX(-1f),
                center.AddY(1f),
                center.AddY(-1f)
            };

            return CheckNeighbors(toCheck);
        }

        public Dictionary<Vector2, Tile> GetDiagonalNeighbors(Vector2 center)
        {
            var toCheck = new Vector2[4]{
                center.Add(-1, -1),
                center.Add(-1, 1),
                center.Add(1, -1),
                center.Add(1, 1)
            };

            return CheckNeighbors(toCheck);
        }

        public Dictionary<Vector2, Tile> GetKingNeighbors(Vector2 center)
        {
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

            return CheckNeighbors(toCheck);
        }

        private Dictionary<Vector2, Tile> CheckNeighbors(Vector2[] toCheck)
        {
            var ret = new Dictionary<Vector2, Tile>();
            Tile next = null;

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
        public Dictionary<Vector2, Tile> GetMovableNeighbors(Tile center, PieceModel piece, Tile dest, bool includeOccupied)
        {
            var ret = GetNeighbors(center.position);

            //filter tiles that are too high/low to move to & are passable
            ret = ret.Where(t =>
                !t.Value.unpassable
                && (isHeightPassable(t.Value, center) || (piece.statuses & Statuses.Flying) != 0)
            ).ToDictionary(k => k.Key, v => v.Value);

            if(!includeOccupied){
                //filter out tiles with enemies on them that aren't the destination
                ret = ret.Where(t =>
                    (dest != null && t.Key == dest.position) ||
                    !pieces.Pieces.Any(m => m.tilePosition == t.Key && m.playerId != piece.playerId)
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
            }

            return ret;
        }

        public List<Tile> CleavePositions(Vector2 position, Direction direction)
        {
            List<Vector2> positions = null;
            switch(direction)
            {
                case Direction.North:
                    positions = new List<Vector2>(){position.Add(1,1), position.Add(-1,1)};
                    break;
                case Direction.East:
                    positions = new List<Vector2>(){position.Add(1,-1), position.Add(1,1)};
                    break;
                case Direction.South:
                    positions = new List<Vector2>(){position.Add(1,-1), position.Add(-1,-1)};
                    break;
                case Direction.West:
                    positions = new List<Vector2>(){position.Add(-1,-1), position.Add(-1,1)};
                    break;
            }
            var attackTiles = new List<Tile>();
            positions.ForEach(p => {
                var tile = mapModel.tiles.Get(p);
                if(tile != null) attackTiles.Add(tile);
            });
            return attackTiles;
        }

        public List<Tile> PiercePositions(Vector2 position, Direction direction)
        {
            List<Vector2> positions = null;
            switch(direction)
            {
                case Direction.North:
                    positions = new List<Vector2>(){ position.AddY(2), position.AddY(3)};
                    break;
                case Direction.East:
                    positions = new List<Vector2>(){position.AddX(2), position.AddX(3)};
                    break;
                case Direction.South:
                    positions = new List<Vector2>(){position.AddY(-2), position.AddY(-3)};
                    break;
                case Direction.West:
                    positions = new List<Vector2>(){position.AddX(-2), position.AddX(-3)};
                    break;
            }
            var attackTiles = new List<Tile>();
            positions.ForEach(p => {
                var tile = mapModel.tiles.Get(p);
                if(tile != null) attackTiles.Add(tile);
            });
            return attackTiles;
        }

        //If a piece goes from start to end what direction will they be facing at end?
        public Direction FaceDirection(Vector2 start, Vector2 end)
        {
            var difference = end - start;
            var targetDirection = Direction.South;
            if (difference.x > 0)
            {
                targetDirection = Direction.East;
            }
            else if (difference.x < 0)
            {
                targetDirection = Direction.West;
            }
            else if (difference.y > 0)
            {
                targetDirection = Direction.North;
            }
            else if (difference.y < 0)
            {
                targetDirection = Direction.South;
            }
            return targetDirection;
        }
    }
}

