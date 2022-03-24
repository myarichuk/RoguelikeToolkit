using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Utf8Json;
using Utf8Json.Resolvers;

namespace RoguelikeToolkit.Keyboard
{
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

		public static KeyMappingStore<KeyEnum, TData> LoadFrom(string filename)
		{
			using var fs = File.OpenRead(filename);
			var keyCombinations = JsonSerializer.Deserialize<List<KeyMapping<KeyEnum, TData>>>(fs, StandardResolver.Default);

			return new KeyMappingStore<KeyEnum, TData>(keyCombinations);
		}

		public void SaveTo(string filename)
		{
			using var fs = File.Create(filename);
			JsonSerializer.Serialize(fs, _keyCombinations);
			fs.Flush();
		}

		public IEnumerable<TData> FetchActionIDsFor(KeyEnum key) =>
			_keyCombinations.Where(kc => kc.FirstKey.Equals(key))
							.Select(kc => kc.Action)
							.Distinct();

		public IEnumerable<TData> FetchActionIDsFor(KeyEnum key1, KeyEnum key2) =>
			_keyCombinations.Where(kc => kc.FirstKey.Equals(key1) &&
															  kc.SecondKey.Equals(key2))
							.Select(kc => kc.Action)
							.Distinct();

		public IEnumerable<TData> FetchActionIDsFor(KeyEnum key1, KeyEnum key2, KeyEnum key3) =>
			_keyCombinations.Where(kc => kc.FirstKey.Equals(key1) &&
															  kc.SecondKey.Equals(key2) &&
															  kc.ThirdKey.Equals(key3))
							.Select(kc => kc.Action)
							.Distinct();

		public bool HasMappingFor(KeyEnum key) =>
			_keyCombinations.Any(kc =>
				kc.FirstKey.Equals(key) && kc.SecondKey.Equals(DefaultValue) && kc.ThirdKey.Equals(DefaultValue));

		public bool HasMappingFor(KeyEnum key1, KeyEnum key2) =>
			_keyCombinations.Any(kc =>
				kc.FirstKey.Equals(key1) && kc.SecondKey.Equals(key2) && kc.ThirdKey.Equals(DefaultValue));

		public bool HasMappingFor(KeyEnum key1, KeyEnum key2, KeyEnum key3) =>
			_keyCombinations.Any(kc =>
				kc.FirstKey.Equals(key1) && kc.SecondKey.Equals(key2) && kc.ThirdKey.Equals(key3));


		public void AddMapping(KeyEnum key, in TData actionId) =>
			_keyCombinations.Add(new KeyMapping<KeyEnum, TData>
			{
				FirstKey = key,
				Action = actionId
			});

		public void AddMapping(KeyEnum key1, KeyEnum key2, in TData actionId) =>
			_keyCombinations.Add(new KeyMapping<KeyEnum, TData>
			{
				FirstKey = key1,
				SecondKey = key2,
				Action = actionId
			});

		public void AddMapping(KeyEnum key1, KeyEnum key2, KeyEnum key3, in TData actionId) =>
			_keyCombinations.Add(new KeyMapping<KeyEnum, TData>
			{
				FirstKey = key1,
				SecondKey = key2,
				ThirdKey = key3,
				Action = actionId
			});
	}
}
