using UnityEngine;

namespace ctac
{
    public static class GameObjectExtensions
    {
        public static void DestroyChildren(this Transform root)
        {
            int childCount = root.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                GameObject.Destroy(root.GetChild(i).gameObject);
            }
        }
    }
}

