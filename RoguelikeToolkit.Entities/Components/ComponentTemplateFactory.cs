﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace RoguelikeToolkit.Entities
{
    public struct ComponentFactoryOptions
    {
        public bool IgnoreMissingFields;

        public bool IgnoreInvalidEnumFields;
    }

    public class ComponentFactory
    {
        private readonly Type[] _componentTypes;
        private readonly MapperRepository _mapperRepository;

        public ComponentFactory(Type[] componentTypes = null, MapperRepository mapperRepository = null)
        {
            _componentTypes = componentTypes ?? Array.Empty<Type>();
            _mapperRepository = mapperRepository ?? new MapperRepository(new ThisAssemblyResolver());
        }

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

            var instance = CreateInstanceInner(type, options, template.PropertyValues);

            return instance;
        }

        protected object CreateInstanceInner(Type destType, EntityFactoryOptions options, IReadOnlyDictionary<string, object> data)
        {
            for (int i = 0; i < _mapperRepository.TypeMappers.Count; i++)
            {
                var mapper = _mapperRepository.TypeMappers[i];
                if (mapper.CanMap(destType, data))
                {
                    return mapper.Map(_mapperRepository.PropertyMappers, destType, data,
                        (innerData, innerType) =>
                            CreateInstanceInner(innerType, options, innerData), _componentTypes, options);
                }
            }
            throw new InvalidOperationException($"Failed to create component instance of type {destType.FullName}, no suitable mappers found");
        }
    }
}
