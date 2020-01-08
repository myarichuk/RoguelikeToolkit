using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using Troschuetz.Random;
using Troschuetz.Random.Generators;

namespace RoguelikeToolkit.DiceExpression
{
    public partial class Dice
    {
        private static readonly IGenerator Random = new NR3Q1Generator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int Roll100() => RollAndSum(1, 100);

        internal static int RollAndSum(int numOfDice, int sides)
        {
            var result = 0;
            for (int i = 0; i < numOfDice; i++)
                lock (Random) //NR3Q1Generator is not thread-safe
                    result += Random.Next(1, sides);
            return result;
        }

        internal static IEnumerable<int> Roll(int numOfDice, int sides)
        {
            for (int i = 0; i < numOfDice; i++)
                lock (Random) //NR3Q1Generator is not thread-safe
                    yield return Random.Next(1, sides);
        }

    }
}
