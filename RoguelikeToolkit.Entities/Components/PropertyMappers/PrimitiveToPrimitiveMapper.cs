using System;
using System.Collections.Generic;
using RoguelikeToolkit.Entities.Components.TypeMappers;

namespace RoguelikeToolkit.Entities.Components.PropertyMappers
{
    public class PrimitiveToPrimitiveMapper : IPropertyMapper
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

            return destType.IsPrimitive && !destType.IsEnum && value is not string;
        }

        public object Map(IReadOnlyList<IPropertyMapper> propertyMappers, Type valueType, object val)
        {
            try
            {
                return Convert.ChangeType(val, valueType);
            }
            catch (OverflowException e)
            {
                throw new InvalidOperationException($"Failed to convert {val} to {valueType}, this is most likely due to incorrect component type being specified. ", e);
            }
            catch (FormatException e)
            {
                throw new InvalidOperationException($"Failed to convert {val} to {valueType}, this is most likely due to weird value format that wasn't recognized. ", e);
            }
            catch (InvalidCastException e)
            {
                throw new InvalidOperationException($"Failed to convert {val} to {valueType}, the conversion is most likely not supported. Try implementing IConvertible to solve this...", e);
            }
        }
    }
}