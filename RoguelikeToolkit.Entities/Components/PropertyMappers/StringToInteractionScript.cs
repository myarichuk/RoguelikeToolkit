using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RoguelikeToolkit.Entities.Components.TypeMappers;
using RoguelikeToolkit.Scripts;

namespace RoguelikeToolkit.Entities.Components.PropertyMappers
{
    public class StringToInteractionScript : IPropertyMapper
    {
        private readonly static ConcurrentDictionary<string, EntityInteractionScript> ScriptCache = new();
        private static Assembly[] ComponentAssemblyCache;

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

            if (ScriptUtils.TryGetScript(script, out var scriptFromFile))
                script = scriptFromFile;

            return ScriptCache.GetOrAdd(script, scriptAsKey =>
            {
                lock (ScriptCache)
                {
                    if (ComponentAssemblyCache is null && ctr is not null)
                        ComponentAssemblyCache = ctr.Values.Select(t => t.Assembly).Distinct().ToArray();
                }

                return ctr == null
                    ? new EntityInteractionScript(scriptAsKey)
                    : new EntityInteractionScript(scriptAsKey, ComponentAssemblyCache);
            });
        }
    }
}