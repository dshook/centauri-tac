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
    }
}
