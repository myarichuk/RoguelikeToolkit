﻿using RoguelikeToolkit.Entities.Components.TypeMappers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using static RoguelikeToolkit.Entities.ReflectionExtensions;

namespace RoguelikeToolkit.Entities.Components.PropertyMappers
{
    public class ICollectionToICollectionMapper : IPropertyMapper
    {
        private readonly static ConcurrentDictionary<Type, ObjectActivator> _ctorsPerType = new();
        private readonly static Lazy<List<IPropertyMapper>> _propertyMappers = new(() => Mappers.Instance.PropertyMappers.OrderBy(x => x.Priority).ToList());

        public int Priority => 15;

        public bool CanMap(Type destType, object value)
        {
            if (destType is null)
                throw new ArgumentNullException(nameof(destType));

            if (value is null)
                throw new ArgumentNullException(nameof(value));

            return destType.IsICollection() && (value?.GetType().IsICollection() ?? false);
        }

        public object Map(Type destType, object value)
        {
            var instanceCreator = _ctorsPerType.GetOrAdd(destType, type =>
            {
                var ctor = type.GetConstructor(Array.Empty<Type>());
                return ctor.CreateActivator();
            });

            var instance = (dynamic)instanceCreator();
            var srcCollection = ((dynamic)value);

            if (srcCollection?.Count == 0)
                return instance;

            var converter = _propertyMappers.Value.FirstOrDefault(c => c.CanMap(destType.GenericTypeArguments[0], srcCollection[0]));
            if (converter == null)
                    throw new InvalidOperationException($"Cannot map between collections, couldn't find appropriate mapper between {destType.GenericTypeArguments[0].FullName} and {((dynamic)value)[0].GetType().FullName}");

            //we already know value and destination type are ICollection<T>, so...
            foreach (var item in srcCollection)
                instance.Add(converter.Map(destType.GenericTypeArguments[0], item));

            return instance;
        }
    }
}
