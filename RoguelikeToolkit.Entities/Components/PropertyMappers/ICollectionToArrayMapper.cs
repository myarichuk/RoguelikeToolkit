﻿using RoguelikeToolkit.Entities.Components.TypeMappers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using static RoguelikeToolkit.Entities.ReflectionExtensions;

namespace RoguelikeToolkit.Entities.Components.PropertyMappers
{
    public class ICollectionToArrayMapper : IPropertyMapper
    {
        private readonly ConcurrentDictionary<Type, ObjectActivator> _arrayConstructorsPerType = new();
        private static readonly Lazy<List<IPropertyMapper>> _propertyMappers = new(() => Mappers.Instance.PropertyMappers.OrderBy(x => x.Priority).ToList());

        public int Priority => 8;

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

            return destType.IsArray && (value?.GetType().IsICollection() ?? false);
        }

        public object Map(Type destType, object value)
        {
            var activator = _arrayConstructorsPerType.GetOrAdd(destType, type =>
            {
                var ctor = type.GetConstructor(new[] { typeof(int) });
                return ctor.CreateActivator();
            });
            var srcCollection = (dynamic)value;

            int srcCollectionCount = (int)srcCollection?.Count;
            var array = (dynamic)activator.Invoke(srcCollectionCount);

            if (srcCollectionCount == 0)
            {
                return array; //nothing to do, empty array is empty...
            }

            var itemType = destType.GetElementType();

            var converter = _propertyMappers.Value.FirstOrDefault(c => c.CanMap(itemType, srcCollection[0]));
            if (converter == null)
            {
                throw new InvalidOperationException($"Cannot map between collections, couldn't find appropriate mapper between {destType.GenericTypeArguments[0].FullName} and {((dynamic)value)[0].GetType().FullName}");
            }

            for (var i = 0; i < srcCollectionCount; i++)
            {
                array[i] = converter.Map(itemType, srcCollection[i]);
            }

            return array;
        }
    }
}