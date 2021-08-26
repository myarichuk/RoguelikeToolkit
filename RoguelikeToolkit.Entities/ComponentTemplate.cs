using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace RoguelikeToolkit.Entities
{
    public class ComponentTemplate
    {
        private readonly IReadOnlyDictionary<string, object> _propertyValues;

        internal IReadOnlyDictionary<string, object> PropertyValues => _propertyValues;

        public ComponentTemplate(IDictionary<string, object> propertyValues) => 
            _propertyValues = BuildEmbeddedTemplates(propertyValues);

        public bool IsValueComponent => 
            _propertyValues.Count == 1 && 
            _propertyValues.ContainsKey(nameof(IValueComponent<object>.Value));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ComponentTemplate ParseFromFile(string jsonFile)
        {
            if (!File.Exists(jsonFile))
                ThrowNotFound(jsonFile);

            //not very efficient (memory allocations!) but more efficiency is unlikely to be needed *here*
            return ParseFromString(File.ReadAllText(jsonFile));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ComponentTemplate(string json) => ParseFromString(json);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ComponentTemplate ParseFromString(string json)
        {
            //note: Utf8Json embeds IReadOnlyDictionary<string, object> for embedded objects
            // *IF* it is set to deserialize as *dynamic*
            if (!json.TryDeserialize(out var data))
                throw new InvalidDataException("Failed to parse malformed json");

            return new ComponentTemplate(data);
        }

        #region Helpers

        private static void ThrowNotFound(string jsonFile) => 
            throw new ArgumentException($"The file {jsonFile} was not found", nameof(jsonFile));

        private static IReadOnlyDictionary<string, object> BuildEmbeddedTemplates(IDictionary<string, object> data)
        {
            var dataWithEmbeddedTemplates = new Dictionary<string, object>();
            foreach (var item in data)
            {
                switch (item.Value)
                {
                    case IDictionary<string, object> embedded:
                        dataWithEmbeddedTemplates.Add(item.Key, new ComponentTemplate(embedded));
                        break;
                    default:
                        dataWithEmbeddedTemplates.Add(item.Key, item.Value);
                        break;
                }
            }
            return dataWithEmbeddedTemplates;
        }

        #endregion
    }
}
