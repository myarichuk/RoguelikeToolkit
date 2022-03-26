using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Utf8Json;
using Utf8Json.Resolvers;

namespace RoguelikeToolkit.Keyboard
{
	public class KeyMappingStore<KeyEnum> : KeyMappingStore<KeyEnum, string> where KeyEnum : Enum
	{
	}

	//naive implementation, but shouldn't be a problem since there will be a few shortcuts..
	//should be optimized if it proves to be a bottleneck
	public class KeyMappingStore<KeyEnum,TData> where KeyEnum : Enum
	{
		private readonly List<KeyMapping<KeyEnum, TData>> _keyCombinations;
		private static readonly KeyEnum DefaultValue = (KeyEnum)Enum.GetValues(typeof(KeyEnum)).GetValue(0);
		public KeyMappingStore() =>
			_keyCombinations = new List<KeyMapping<KeyEnum, TData>>();

		internal KeyMappingStore(List<KeyMapping<KeyEnum, TData>> keyCombinations) =>
			_keyCombinations = keyCombinations;

		/// <summary>
		/// Load key mappings from a JSON file
		/// </summary>
		/// <param name="filename">Path of the JSON file</param>
		/// <returns>Key mappings repository</returns>
		public static KeyMappingStore<KeyEnum, TData> LoadFrom(string filename)
		{
			using var fs = File.OpenRead(filename);
			var keyCombinations = JsonSerializer.Deserialize<List<KeyMapping<KeyEnum, TData>>>(fs, StandardResolver.Default);

			return new KeyMappingStore<KeyEnum, TData>(keyCombinations);
		}

		/// <summary>
		/// Serialize key mappings into JSON file
		/// </summary>
		/// <param name="filename">Path of the JSON file</param>
		public void SaveTo(string filename)
		{
			using var fs = File.Create(filename);
			JsonSerializer.Serialize(fs, _keyCombinations);
			fs.Flush();
		}

		/// <summary>
		/// Fetch action IDs that map to a specified key
		/// </summary>
		/// <param name="key">Key mapping</param>
		/// <remarks>may return duplicates as multiple key combinations can map to the same action</remarks>
		/// <returns>IEnumerable of action IDs</returns>
		public IEnumerable<TData> FetchActionIDsFor(KeyEnum key) =>
			_keyCombinations.Where(kc => kc.FirstKey.Equals(key))
							.Select(kc => kc.Action)
							.Distinct();

		/// <summary>
		/// Fetch action IDs that map to a specified key combination
		/// </summary>
		/// <param name="key1">Key mapping 1</param>
		/// <param name="key2">Key mapping 2</param>
		/// <remarks>may return duplicates as multiple key combinations can map to the same action</remarks>
		/// <returns>IEnumerable of action IDs</returns>
		public IEnumerable<TData> FetchActionIDsFor(KeyEnum key1, KeyEnum key2) =>
			_keyCombinations.Where(kc => kc.FirstKey.Equals(key1) &&
															  kc.SecondKey.Equals(key2))
							.Select(kc => kc.Action)
							.Distinct();

		/// <summary>
		/// Fetch action IDs that map to a specified key combination
		/// </summary>
		/// <param name="key1">Key mapping 1</param>
		/// <param name="key2">Key mapping 2</param>
		/// <param name="key3">Key mapping 3</param>
		/// <remarks>may return duplicates as multiple key combinations can map to the same action</remarks>
		/// <returns>IEnumerable of action IDs</returns>
		public IEnumerable<TData> FetchActionIDsFor(KeyEnum key1, KeyEnum key2, KeyEnum key3) =>
			_keyCombinations.Where(kc => kc.FirstKey.Equals(key1) &&
															  kc.SecondKey.Equals(key2) &&
															  kc.ThirdKey.Equals(key3))
							.Select(kc => kc.Action)
							.Distinct();

		/// <summary>
		/// Checks whether there are any mappings for a specified key
		/// </summary>
		public bool HasMappingFor(KeyEnum key) =>
			_keyCombinations.Any(kc =>
				kc.FirstKey.Equals(key) && kc.SecondKey.Equals(DefaultValue) && kc.ThirdKey.Equals(DefaultValue));

		/// <summary>
		/// Checks whether there are any mappings for a specified key combination
		/// </summary>
		public bool HasMappingFor(KeyEnum key1, KeyEnum key2) =>
			_keyCombinations.Any(kc =>
				kc.FirstKey.Equals(key1) && kc.SecondKey.Equals(key2) && kc.ThirdKey.Equals(DefaultValue));

		/// <summary>
		/// Checks whether there are any mappings for a specified key combination
		/// </summary>
		public bool HasMappingFor(KeyEnum key1, KeyEnum key2, KeyEnum key3) =>
			_keyCombinations.Any(kc =>
				kc.FirstKey.Equals(key1) && kc.SecondKey.Equals(key2) && kc.ThirdKey.Equals(key3));

		/// <summary>
		/// Add key mapping
		/// </summary>
		/// <param name="key">key enum</param>
		/// <param name="actionId">action id</param>
		public void AddMapping(KeyEnum key, in TData actionId) =>
			_keyCombinations.Add(new KeyMapping<KeyEnum, TData>
			{
				FirstKey = key,
				Action = actionId
			});

		/// <summary>
		/// Add key mapping
		/// </summary>
		/// <param name="key1">first key enum</param>
		/// <param name="key2">second key enum</param>
		/// <param name="actionId">action id</param>
		public void AddMapping(KeyEnum key1, KeyEnum key2, in TData actionId) =>
			_keyCombinations.Add(new KeyMapping<KeyEnum, TData>
			{
				FirstKey = key1,
				SecondKey = key2,
				Action = actionId
			});

		/// <summary>
		/// Add key mapping
		/// </summary>
		/// <param name="key1">first key enum</param>
		/// <param name="key2">second key enum</param>
		/// <param name="key3">third key enum</param>
		/// <param name="actionId">action id</param>
		public void AddMapping(KeyEnum key1, KeyEnum key2, KeyEnum key3, in TData actionId) =>
			_keyCombinations.Add(new KeyMapping<KeyEnum, TData>
			{
				FirstKey = key1,
				SecondKey = key2,
				ThirdKey = key3,
				Action = actionId
			});

		/// <summary>
		/// Remove all key combinations that match the specified key
		/// </summary>
		/// <remarks>note that the two other keys must not be set for the combination to be deleted</remarks>
		/// <param name="key">Key to match</param>
		/// <returns>Number of key combinations to be removed</returns>
		public int RemoveMappingOf(KeyEnum key) =>
			_keyCombinations.RemoveAll(
				kc => kc.FirstKey.Equals(key) &&
					kc.SecondKey.Equals(DefaultValue) &&
					kc.ThirdKey.Equals(DefaultValue));

		/// <summary>
		/// Remove all key combinations that match the specified key
		/// </summary>
		/// <remarks>note that the third key must not be set for the combination to be deleted</remarks>
		/// <param name="key1">Key 1 to match</param>
		/// <param name="key2">Key 2 to match</param>
		/// <returns>Number of key combinations to be removed</returns>
		public int RemoveMappingOf(KeyEnum key1, KeyEnum key2) =>
			_keyCombinations.RemoveAll(
				kc => kc.FirstKey.Equals(key1) &&
					kc.SecondKey.Equals(key2) &&
					kc.ThirdKey.Equals(DefaultValue));

		/// <summary>
		/// Remove all key combinations that match the specified key
		/// </summary>
		/// <remarks>note that the third key must not be set for the combination to be deleted</remarks>
		/// <param name="key1">Key 1 to match</param>
		/// <param name="key2">Key 2 to match</param>
		/// <param name="key3">Key 3 to match</param>
		/// <returns>Number of key combinations to be removed</returns>
		public int RemoveMappingOf(KeyEnum key1, KeyEnum key2, KeyEnum key3) =>
			_keyCombinations.RemoveAll(
				kc => kc.FirstKey.Equals(key1) &&
					kc.SecondKey.Equals(key2) &&
					kc.ThirdKey.Equals(key3));
	}
}
