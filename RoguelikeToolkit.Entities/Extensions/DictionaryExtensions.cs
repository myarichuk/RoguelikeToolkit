using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Runtime.CompilerServices;

namespace RoguelikeToolkit.Entities
{
	public static class DictionaryExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void MergeWith<TKey, TValue>(this IDictionary<TKey, TValue> dict, IDictionary<TKey, TValue> dictToMerge)
		{
			if (dictToMerge == null) //just in case
				return;

			foreach (var kvp in dictToMerge)
				dict.AddIfNotExists(kvp.Key, kvp.Value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void MergeWith<TKey, TValue>(this IDictionary<TKey, TValue> dict, IReadOnlyDictionary<TKey, TValue> dictToMerge)
		{
			if (dictToMerge == null) //just in case
				return;

			foreach (var kvp in dictToMerge)
				dict.AddIfNotExists(kvp.Key, kvp.Value);
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void AddIfNotExists<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue val)
		{
			if (!dict.ContainsKey(key))
				dict.Add(key, val);
		}

		public static dynamic ToExpando<TValue>(this IReadOnlyDictionary<string, TValue> dict)
		{
			var result = (IDictionary<string, object>)new ExpandoObject();

			foreach (var kvp in dict)
				result.Add(kvp.Key, kvp.Value);

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static dynamic ToExpando<TValue>(this IDictionary<string, TValue> dict) =>
			ToExpando((IReadOnlyDictionary<string, TValue>)dict);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, Func<TKey, TValue> valueFactory)
		{
			if (dict.TryGetValue(key, out var value))
			{
				return value;
			}

			var newValue = valueFactory(key);
			dict.Add(key, newValue);

			return newValue;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void AddOrSet<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, Func<TValue, TValue> mutator)
		{
			Debug.Assert(mutator != null, nameof(mutator) + " != null");

			if (dict.ContainsKey(key))
			{
				dict[key] = mutator(dict[key]);
			}
			else
			{
				dict.Add(key, mutator(default));
			}
		}
	}
}
