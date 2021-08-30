using System;

namespace RoguelikeToolkit.Entities.Components.TypeMappers
{
    public interface IPropertyMapper
    {
        int Priority { get; }

        bool CanMap(Type destType, object value);
        
        object Map(Type destType, object value);
    }
}
