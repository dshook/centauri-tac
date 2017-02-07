using UnityEngine;

namespace ctac
{
    public static class VectorExtensions
    {
        public static Vector2 AddX(this Vector2 vec, float amt)
        {
            return new Vector2(vec.x + amt, vec.y);
        }

        public static Vector2 AddY(this Vector2 vec, float amt)
        {
            return new Vector2(vec.x, vec.y + amt);
        }
        
        public static Vector2 Add(this Vector2 vec, float xAmt, float yAmt)
        {
            return new Vector2(vec.x + xAmt, vec.y + yAmt);
        }

        public static Vector2 ToTileCoordinates(this Vector3 vec)
        {
            return new Vector2((int)vec.x, (int)vec.z);
        }

        public static PositionModel ToPositionModel(this Vector2 position)
        {
            return new PositionModel(position);
        }

        public static Vector3 SetX(this Vector3 vec, float value)
        {
            vec.Set(value, vec.y, vec.z);
            return vec;
        }

        public static Vector3 SetY(this Vector3 vec, float value)
        {
            vec.Set(vec.x, value, vec.z);
            return vec;
        }

        public static Vector3 SetZ(this Vector3 vec, float value)
        {
            vec.Set(vec.x, vec.y, value);
            return vec;
        }

        public static Vector3 AddX(this Vector3 vec, float amt)
        {
            return new Vector3(vec.x + amt, vec.y, vec.z);
        }

        public static Vector3 AddY(this Vector3 vec, float amt)
        {
            return new Vector3(vec.x, vec.y + amt, vec.z);
        }

        public static Vector3 AddZ(this Vector3 vec, float amt)
        {
            return new Vector3(vec.x, vec.y, vec.z + amt);
        }
        
        public static Vector3 Add(this Vector3 vec, float xAmt, float yAmt, float zAmt)
        {
            return new Vector3(vec.x + xAmt, vec.y + yAmt, vec.z + zAmt);
        }
    }
}
