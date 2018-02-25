using System;
using System.Collections.Generic;
using UnityEngine;

namespace ctac
{
    public static class CollectionExtensions
    {
        public static TValue Get<TKey,TValue>(this Dictionary<TKey, TValue> dict, TKey key)
        {
            TValue val;
            dict.TryGetValue(key, out val);
            return val;
        }

        public static TValue Sample<TValue>(this List<TValue> list)
        {
            return list[UnityEngine.Random.Range(0, list.Count - 1)];
        }

        public static T Sample<T>(this T[] list)
        {
            return list[UnityEngine.Random.Range(0, list.Length - 1)];
        }
    }
}