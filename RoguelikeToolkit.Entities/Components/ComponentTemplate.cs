using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        private static IReadOnlyDictionary<string, object> BuildEmbeddedTemplates(IDictionary<string, object> data) =>
                    data.ToDictionary(item => item.Key,
                        item => item.Value is IDictionary<string, object> embedded ? 
                            new ComponentTemplate(embedded) : 
                            item.Value);

        #endregion
    }
}
