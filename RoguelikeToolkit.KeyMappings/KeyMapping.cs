using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RoguelikeToolkit.KeyMapping
{
    public class KeyMapping<KeyEnum, ActionEnum>
        where KeyEnum : Enum
        where ActionEnum : Enum
    {
        private readonly ConcurrentDictionary<KeyCombination<KeyEnum>, ActionEnum> _keymap;
        private readonly static JsonSerializerOptions _serializerOptions;

        static KeyMapping()
        {
            _serializerOptions = new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                PropertyNameCaseInsensitive = true
            };

            _serializerOptions.Converters.Add(new JsonStringEnumConverter());
        }

        public bool TrySetKeyAction(string id, ActionEnum action)
        {
            var key = _keymap.FirstOrDefault(x => x.Key.Id.Equals(id, StringComparison.InvariantCultureIgnoreCase)).Key;
            if (key == null)
                return false;

            _keymap.AddOrUpdate(key, action, (existingKey, _) => action);
            return true;
        }

        public bool TryGetActionFor(string id, out ActionEnum action)
        {
            action = default;
            var kvp = _keymap.FirstOrDefault(x => x.Key.Id.Equals(id, StringComparison.InvariantCultureIgnoreCase));
            if (kvp.Key == null)
                return false;

            action = kvp.Value;
            return true;
        }

        public static KeyMapping<KeyEnum, ActionEnum> FromFile(string filename)
        {
            if (!File.Exists(filename))
                throw new FileNotFoundException($"Failed to read keymappings from {filename}, couldn't find it...");

            return FromJson(File.ReadAllText(filename));
        }

        public static KeyMapping<KeyEnum, ActionEnum> FromJson(string json) =>
            JsonSerializer.Deserialize<KeyMapping<KeyEnum, ActionEnum>>(json, _serializerOptions);

        public static KeyMapping<KeyEnum, ActionEnum> FromBytes(in ReadOnlySpan<byte> jsonBytes) =>
            JsonSerializer.Deserialize<KeyMapping<KeyEnum, ActionEnum>>(jsonBytes, _serializerOptions);
    }
}
