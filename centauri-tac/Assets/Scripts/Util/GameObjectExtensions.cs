using UnityEngine;

namespace ctac
{
    public static class GameObjectExtensions
    {
        public static void DestroyChildren(this Transform root, bool immediate = false)
        {
            int childCount = root.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                if (immediate)
                {
                    GameObject.DestroyImmediate(root.GetChild(i).gameObject);
                }
                else
                {
                    GameObject.Destroy(root.GetChild(i).gameObject);
                }
            }
        }
    }
}

