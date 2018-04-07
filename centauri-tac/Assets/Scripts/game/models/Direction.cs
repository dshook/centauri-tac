using System.Collections.Generic;
using UnityEngine;

namespace ctac
{
    public enum Direction
    {
        North = 1,
        East = 2,
        South = 3,
        West = 4
    }

    public static class DirectionHelpers
    {
        public static Dictionary<Direction, Vector3> directionAngle = new Dictionary<Direction, Vector3>() {
            {Direction.North, new Vector3(0, 0, 0) },
            {Direction.East, new Vector3(0, 90, 0) },
            {Direction.South, new Vector3(0, 180, 0)},
            {Direction.West, new Vector3(0, 270, 0) }
        };

        public static Vector3 adjacentPosition(Vector2 position, Direction direction, bool isDiagonal = false){
            if(isDiagonal){
                switch(direction)
                {
                    case Direction.North:
                        return position.Add(1,1);
                    case Direction.East:
                        return position.Add(1,-1);
                    case Direction.South:
                        return position.Add(-1,-1);
                    case Direction.West:
                        return position.Add(-1,1);
                }
            }else{
                switch(direction)
                {
                case Direction.North:
                    return position.Add(0,1);
                case Direction.East:
                    return position.Add(1,0);
                case Direction.South:
                    return position.Add(0,-1);
                case Direction.West:
                    return position.Add(-1,0);
                }
            }

            return Vector2.zero;
        }
    }

}
