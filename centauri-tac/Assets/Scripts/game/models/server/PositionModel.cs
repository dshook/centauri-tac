using UnityEngine;

namespace ctac
{
    public class PositionModel
    {
        public int x { get; set; }
        public float y { get; set; }
        public int z { get; set; }

        public Vector3 Vector3
        {
            get
            {
                return new Vector3(x, y, z);
            }
        }
    }
}
