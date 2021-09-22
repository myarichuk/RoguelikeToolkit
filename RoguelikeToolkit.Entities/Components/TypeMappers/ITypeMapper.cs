using System;
using System.Collections.Generic;

namespace RoguelikeToolkit.Entities.Components.TypeMappers
{
    public interface ITypeMapper
    {
        int Priority { get; }

        bool CanMap(Type destType, IReadOnlyDictionary<string, object> data);

        object Map(IReadOnlyList<IPropertyMapper> propertyMappers, Type destType, IReadOnlyDictionary<string, object> data, Func<IReadOnlyDictionary<string, object>, Type, object> createInstance, EntityFactoryOptions options = null);
    }
}
