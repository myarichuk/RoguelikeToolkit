using System;
using System.Collections.Generic;
using System.Text;

namespace RoguelikeToolkit.Common.Dice
{
    //store AST of a dice expression to be evaluated when needed
    public class DiceExpression
    {
        public static int Evaluate(string diceExpression)
        {
            throw new NotImplementedException("TODO: Implement with ANTLR");
        }

        public static bool TryParse(string diceExpression, out Dice dice)
        {
            throw new NotImplementedException("TODO: Implement with ANTLR");
        }
    }
}
