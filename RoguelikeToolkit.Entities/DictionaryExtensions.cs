using System;
using System.Collections.Generic;

namespace RoguelikeToolkit.Entities
{
    public static class DictionaryExtensions
    {
        public static void AddOrSet<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, Func<TValue, TValue> mutator)
        {
            if (dict.ContainsKey(key))
                dict[key] = mutator(dict[key]);
            else
                dict.Add(key, mutator(default));
        }
    }
}
