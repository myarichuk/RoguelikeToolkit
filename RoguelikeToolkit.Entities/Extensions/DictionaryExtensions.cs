using System;
using System.Collections.Generic;
using System.Dynamic;

namespace RoguelikeToolkit.Entities
{
    public static class DictionaryExtensions
    {
        public static ExpandoObject ToExpandoObject(this Dictionary<string, object> dict)
        {
            var result = new ExpandoObject();
            var resultAsDictionary = (IDictionary<string, object>)result;
            foreach(var kvp in dict)
            {
                if(kvp.Value is Dictionary<string, object> valueAsDict)
                    resultAsDictionary.Add(kvp.Key, valueAsDict.ToExpandoObject());
                else
                    resultAsDictionary.Add(kvp.Key, kvp.Value);
            }

            return result;
        }

        public static void AddOrSet<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, Func<TValue, TValue> mutator)
        {
            if (dict.ContainsKey(key))
                dict[key] = mutator(dict[key]);
            else
                dict.Add(key, mutator(default));
        }
    }
}
