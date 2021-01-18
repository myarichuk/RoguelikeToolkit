using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace RoguelikeToolkit.KeyboardActions
{
    public class KeyMapping<KeyEnum, ActionEnum>
        where KeyEnum : Enum
        where ActionEnum : Enum
    {
        private readonly ConcurrentDictionary<KeyEnum, ActionEnum> _keymap;
        private readonly static JsonSerializerOptions _serializerOptions = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            PropertyNameCaseInsensitive = true
        };

        internal KeyMapping(IDictionary<KeyEnum, ActionEnum> keymap) =>
            _keymap = new ConcurrentDictionary<KeyEnum, ActionEnum>(keymap) ?? new ConcurrentDictionary<KeyEnum, ActionEnum>();

        public void SetKeyAction(KeyEnum key, ActionEnum action) =>
            _keymap.AddOrUpdate(key, action, (existingKey, _) => action);

        public bool TryGetActionFor(KeyEnum key, out ActionEnum action) =>
            _keymap.TryGetValue(key, out action);

        public static KeyMapping<KeyEnum, ActionEnum> FromFile(string filename)
        {
            if (!File.Exists(filename))
                throw new FileNotFoundException($"Failed to read keymappings from {filename}, couldn't find it...");

            return FromJson(File.ReadAllText(filename));
        }

        public static KeyMapping<KeyEnum, ActionEnum> FromJson(string json)
        {
            var loadedDict = JsonSerializer.Deserialize<IEnumerable<KeyValuePair<KeyEnum, ActionEnum>>>(json, _serializerOptions);
            return new KeyMapping<KeyEnum, ActionEnum>(loadedDict.ToDictionary(x => x.Key, x => x.Value));
        }

        public static KeyMapping<KeyEnum, ActionEnum> FromBytes(in ReadOnlySpan<byte> jsonBytes)
        {
            var loadedDict = JsonSerializer.Deserialize<IEnumerable<KeyValuePair<KeyEnum, ActionEnum>>>(jsonBytes, _serializerOptions);
            return new KeyMapping<KeyEnum, ActionEnum>(loadedDict.ToDictionary(x => x.Key, x => x.Value));
        }
    }
}
