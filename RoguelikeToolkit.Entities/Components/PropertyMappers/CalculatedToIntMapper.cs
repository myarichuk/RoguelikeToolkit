using System;
using RoguelikeToolkit.DiceExpression;
using RoguelikeToolkit.Entities.Components.TypeMappers;

namespace RoguelikeToolkit.Entities.Components.PropertyMappers
{
    public class CalculatedToIntMapper : IPropertyMapper
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

            return destType.IsNumeric() && value is string str && str.StartsWith("="));
        }

        public object Map(Type destType, object value) =>
            destType == typeof(int) ?
                Dice.Parse(((string)value).Substring(1)).Roll() :
                Convert.ChangeType(Dice.Parse(((string)value).Substring(1)).Roll(), destType);
    }
}