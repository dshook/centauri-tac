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

    public static class DirectionAngle
    {
        public static Dictionary<Direction, Vector3> angle = new Dictionary<Direction, Vector3>() {
            {Direction.North, new Vector3(0, 0, 0) },
            {Direction.East, new Vector3(0, 90, 0) },
            {Direction.South, new Vector3(0, 180, 0)},
            {Direction.West, new Vector3(0, 270, 0) }
        };
    }
}
