using RoguelikeToolkit.Entities.Components.TypeMappers;
using System;

namespace RoguelikeToolkit.Entities.Components.PropertyMappers
{
    public class StringToStringMapper : IPropertyMapper
    {
        public int Priority => 0;

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

            return destType == typeof(string) && value is string;
        }

        public object Map(Type destType, object value) => value?.ToString();
    }
}
