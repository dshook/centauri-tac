using System;
using System.Collections.Generic;

namespace ctac
{
    public static class DictionaryExtensions
    {
        public static TValue Get<TKey,TValue>(this Dictionary<TKey, TValue> dict, TKey key)
        {
            TValue val;
            dict.TryGetValue(key, out val);
            return val;
        }
    }
}
