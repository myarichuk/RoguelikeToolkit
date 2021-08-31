using System;
using System.ComponentModel;
using RoguelikeToolkit.Entities.Components.TypeMappers;

namespace RoguelikeToolkit.Entities.Components.PropertyMappers
{
    public class StringToPrimitiveMapper : IPropertyMapper
    {
        public int Priority => 3;

        public bool CanMap(Type destType, object value)
        {
            return destType.IsPrimitive && value is string;
        }

        public object Map(Type destType, object value)
        {
            var converter = TypeDescriptor.GetConverter(destType);
            if (converter == null || converter.CanConvertFrom(typeof(string)) == false)
                throw new NotSupportedException($"Failed to find type converter suitable for converting string to a {destType.FullName}");

            return converter.ConvertFromString((string)value);
        }
    }
}
