using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace RoguelikeToolkit.Entities
{
    public static class DictionaryExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddIfNotExists<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue val)
        {
            if (!dict.ContainsKey(key))
                dict.Add(key, val);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddOrSet<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, Func<TValue, TValue> mutator)
        {
            if (dict.ContainsKey(key))
                dict[key] = mutator(dict[key]);
            else
                dict.Add(key, mutator(default));
        }
    }
}
