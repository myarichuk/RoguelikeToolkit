using System;
using RoguelikeToolkit.Entities.Components.TypeMappers;

namespace RoguelikeToolkit.Entities.Components.PropertyMappers
{
    public class PrimitiveToStringMapper : IPropertyMapper
    {
        public int Priority => 1;

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

            return destType == typeof(string) && value.GetType().IsPrimitive;
        }

        public object Map(Type destType, object value) => value?.ToString();
    }
}
