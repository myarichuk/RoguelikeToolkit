using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using RoguelikeToolkit.Entities.Components;
using RoguelikeToolkit.Entities.Components.TypeMappers;

namespace RoguelikeToolkit.Entities
{
    public struct ComponentFactoryOptions
    {
        public bool IgnoreMissingFields;
        public bool IgnoreInvalidEnumFields;
    }

    public class ComponentFactory
    {
        private static readonly Lazy<IReadOnlyList<ITypeMapper>> _typeMappers = new(() => Mappers.Instance.TypeMappers.OrderBy(x => x.Priority).ToList());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TInstance CreateInstance<TInstance>(ComponentTemplate template, EntityFactoryOptions options = null) =>
            (TInstance)CreateInstance(typeof(TInstance), options ?? EntityFactoryOptions.Default, template);

        public object CreateInstance(Type type, EntityFactoryOptions options, ComponentTemplate template)
        {
            if (type.IsInterface) //precaution!
            {
                throw new InvalidOperationException($"Cannot create component instance with interface type. (specified type = {type.FullName})");
            }

            if (type == typeof(string) || type.IsPrimitive || type.IsEnum || type.IsPointer || type.IsCOMObject || type.IsByRef)
            {
                throw new InvalidOperationException($"Cannot create component instance with a specified type. The type should *not* be a primitive, enum, by-ref type or a pointer (specified type = {type.FullName})");
            }

            var instance = CreateInstance(type, options, template.PropertyValues);

            return instance;
        }

        protected object CreateInstance(Type destType, EntityFactoryOptions options, IReadOnlyDictionary<string, object> data)
        {
            for (int i = 0; i < _typeMappers.Value.Count; i++)
            {
                var mapper = _typeMappers.Value[i];
                if (mapper.CanMap(destType, data))
                {
                    return mapper.Map(destType, data,
                        (innerData, innerType) =>
                            CreateInstance(innerType, options, innerData), options);
                }
            }
            throw new InvalidOperationException($"Failed to create component instance of type {destType.FullName}, no suitable mappers found");
        }
    }
}
