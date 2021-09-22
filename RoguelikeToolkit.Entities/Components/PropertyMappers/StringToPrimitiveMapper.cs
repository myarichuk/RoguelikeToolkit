using System;
using System.Collections.Generic;
using System.ComponentModel;
using RoguelikeToolkit.Entities.Components.TypeMappers;

namespace RoguelikeToolkit.Entities.Components.PropertyMappers
{
    public class StringToPrimitiveMapper : IPropertyMapper
    {
        //last resort...
        public int Priority => int.MaxValue;

        public bool CanMap(Type destType, object value)
        {
            if (destType is null) throw new ArgumentNullException(nameof(destType));
            if (value is null) throw new ArgumentNullException(nameof(value));

            return destType.IsPrimitive && value is string;
        }

        public object Map(IReadOnlyList<IPropertyMapper> propertyMappers, Type destType, object value = null)
        {
            var converter = TypeDescriptor.GetConverter(destType);
            if (converter == null || converter.CanConvertFrom(typeof(string)) == false)
                throw new NotSupportedException($"Failed to find type converter suitable for converting string to a {destType.FullName}");

            return converter.ConvertFromString((string)value);
        }
    }
}
