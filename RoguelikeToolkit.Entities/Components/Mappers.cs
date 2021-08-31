﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RoguelikeToolkit.Entities.Components.TypeMappers;

namespace RoguelikeToolkit.Entities.Components
{
    internal class Mappers
    {
        private static readonly Lazy<Mappers> _instance = new(() => new());
        private readonly List<IPropertyMapper> _propertyMappers = new();
        private readonly List<ITypeMapper> _typeMappers = new();

        public static Mappers Instance => _instance.Value;

        public IReadOnlyList<IPropertyMapper> PropertyMappers => _propertyMappers;
        public IReadOnlyList<ITypeMapper> TypeMappers => _typeMappers;

        private Mappers()
        {
            var thisAssembly = Assembly.GetExecutingAssembly();
            var propertyMapperTypes = thisAssembly.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IPropertyMapper)));

            foreach (var type in propertyMapperTypes)
            {
                var accessor = MemberAccessor.Get(type);
                _propertyMappers.Add((IPropertyMapper)accessor.CreateNew());
            }

            var typeMapperTypes = thisAssembly.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(ITypeMapper)));

            foreach (var type in typeMapperTypes)
            {
                var accessor = MemberAccessor.Get(type);
                _typeMappers.Add((ITypeMapper)accessor.CreateNew());
            }

        }
    }
}
