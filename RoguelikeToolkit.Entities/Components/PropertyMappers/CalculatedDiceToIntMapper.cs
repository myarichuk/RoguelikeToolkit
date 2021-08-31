using System;
using RoguelikeToolkit.DiceExpression;
using RoguelikeToolkit.Entities.Components.TypeMappers;

namespace RoguelikeToolkit.Entities.Components.PropertyMappers
{
    public class CalculatedDiceToIntMapper : IPropertyMapper
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

            if (destType.IsNumeric() && value is string str && str.StartsWith("="))
            {
                try
                {
                    //check valid expression
                    _ = Dice.Parse(str.Substring(1), true);
                }
                catch (Exception)
                {
                    return false;
                }

                return true;
            }

            return false;
        }

        public object Map(Type destType, object value) =>
            destType == typeof(int) ?
                Dice.Parse(((string)value).Substring(1)).Roll() :
                Convert.ChangeType(Dice.Parse(((string)value).Substring(1)).Roll(), destType);
    }
}