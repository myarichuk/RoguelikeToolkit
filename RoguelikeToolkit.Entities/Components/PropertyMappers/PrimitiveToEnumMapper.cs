using System;
using System.Collections.Generic;
using RoguelikeToolkit.Entities.Components.TypeMappers;

namespace RoguelikeToolkit.Entities.Components.PropertyMappers
{
    public class PrimitiveToEnumMapper : IPropertyMapper
    {
        public int Priority => 2;

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

            return destType.IsEnum && value is IConvertible;
        }

        public object Map(IReadOnlyList<IPropertyMapper> propertyMappers, Type destType, object value, ComponentTypeRepository ctr = null) =>
            Enum.Parse(destType, (string)Convert.ChangeType(value, typeof(string)));
    }
}
