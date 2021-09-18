using System;
using System.Collections.Generic;

namespace RoguelikeToolkit.Entities.Components.TypeMappers
{
    public interface IPropertyMapper
    {
        int Priority { get; }

        bool CanMap(Type destType, object value);

        object Map(IReadOnlyList<IPropertyMapper> propertyMappers, Type destType, object value, ComponentTypeRepository ctr);
    }
}
