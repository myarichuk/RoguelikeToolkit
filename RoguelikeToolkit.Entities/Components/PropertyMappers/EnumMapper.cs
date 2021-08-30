using RoguelikeToolkit.Entities.Components.TypeMappers;
using System;

namespace RoguelikeToolkit.Entities.Components.PropertyMappers
{
    public class EnumFromPrimitiveTypeMapper : IPropertyMapper
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

        public object Map(Type destType, object value) =>
            Enum.Parse(destType, (string)Convert.ChangeType(value, typeof(string)));
    }
}
