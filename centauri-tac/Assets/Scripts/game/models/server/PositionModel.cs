using UnityEngine;
using Newtonsoft.Json;

namespace ctac
{
    public class PositionModel
    {
        public int x { get; set; }
        public float y { get; set; }
        public int z { get; set; }

        public PositionModel(Vector2 vec2)
        {
            x = (int)vec2.x;
            z = (int)vec2.y;
        }

        [JsonIgnore]
        public Vector3 Vector3
        {
            get
            {
                return new Vector3(x, y, z);
            }
        }

        public override string ToString()
        {
            return string.Format("({0}, {1:0.00}, {2})", x, y, z);
        }
    }
}
