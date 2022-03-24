using System.Collections.Generic;

namespace RoguelikeToolkit.Keyboard
{
	internal static class Extensions
	{
		public static bool TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value)
		{
			if(dict?.TryGetValue(key, out _) ?? false)
				return false;

			dict.Add(key, value);
			return true;
		}

		public static void AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value)
		{
			if (dict == null)
				return;

			if(dict.ContainsKey(key))
				dict[key] = value;
			else
				dict.Add(key, value);
		}
	}
}
