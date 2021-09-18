using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using RoguelikeToolkit.Entities.Components.TypeMappers;
using RoguelikeToolkit.Scripts;

namespace RoguelikeToolkit.Entities.Components.PropertyMappers
{
    public class StringToInteractionScript : IPropertyMapper
    {
        private readonly static ConcurrentDictionary<string, EntityInteractionScript> ScriptCache = new();
        public int Priority => 5;

        public bool CanMap(Type destType, object value)
        {
            if (destType is null)
            {
                throw new ArgumentNullException(nameof(destType));
            }

            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return (destType == typeof(EntityInteractionScript) ||
                    destType == typeof(EntityScript<EntityInteractionParam>)) &&
                    value is string;
        }

        public object Map(IReadOnlyList<IPropertyMapper> propertyMappers, Type destType, object value, ComponentTypeRepository ctr = null)
        {
            var script = (string)value;
            return ScriptCache.GetOrAdd(script, scriptAsKey => new EntityInteractionScript(scriptAsKey));
        }
    }
}