using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RoguelikeToolkit.Entities.Components.TypeMappers;
using RoguelikeToolkit.Scripts;

namespace RoguelikeToolkit.Entities.Components.PropertyMappers
{
    public class StringToComponentScript : IPropertyMapper
    {
        private readonly static ConcurrentDictionary<string, EntityComponentScript> ScriptCache = new();

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

            return destType == typeof(EntityComponentScript) && value is string;
        }

        public object Map(IReadOnlyList<IPropertyMapper> propertyMappers, Type destType, object value, Type[] componentTypes)
        {
            var script = (string)value;
            if (ScriptUtils.TryGetScript(script, out var scriptFromFile))
                script = scriptFromFile;

            return ScriptCache.GetOrAdd(script, scriptAsKey => new EntityComponentScript(scriptAsKey, componentTypes));
        }
    }
}
