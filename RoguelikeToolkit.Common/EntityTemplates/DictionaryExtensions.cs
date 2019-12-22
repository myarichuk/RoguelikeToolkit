using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace RoguelikeToolkit.Common.EntityTemplates
{
    public static class DictionaryExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddOrReplace<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue val)
        {
            if (!dict.ContainsKey(key))
                dict.Add(key, val);
            else
                dict[key] = val;
        }
    }
}
