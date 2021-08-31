using System;
using RoguelikeToolkit.DiceExpression;

namespace RoguelikeToolkit.Entities.Components.PropertyMappers
{
    public class CalculatedDiceToIntMapper
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

            if(destType == typeof(int) && value is string str && str.StartsWith("="))
            {
                try
                {
                    //check valid expression
                    _ = Dice.Parse(str, true);
                }
                catch(Exception)
                {
                    return false;
                }

                return true;
            }

            return false;
        }

        public object Map(Type destType, object value) => Dice.Parse(value?.ToString() ?? throw new InvalidOperationException("Cannot parse empty string into dice expression!"));
    }

    public class StringToDiceMapper
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

            return destType == typeof(Dice) && value is string;
        }

        public object Map(Type destType, object value) => Dice.Parse(value?.ToString() ?? throw new InvalidOperationException("Cannot parse empty string into dice expression!"));
    }
}
