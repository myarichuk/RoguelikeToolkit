using RandN;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using RandN.Compat;

namespace RoguelikeToolkit.DiceExpression
{
    public partial class Dice
    {
        //NR3Q1Generator is not thread-safe
        private static readonly ThreadLocal<RandomShim<SmallRng>> Random = new ThreadLocal<RandomShim<SmallRng>>(() => RandomShim.Create(SmallRng.Create()));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int Roll100() => RollAndSum(1, 100);

        internal static int RollAndSum(int numOfDice, int sides)
        {
            var result = 0;			

			for (int i = 0; i < numOfDice; i++)
            {
                result += Random.Value.Next(1, sides);
            }

            return result;
        }

        internal static IEnumerable<int> Roll(int numOfDice, int sides)
        {
            for (int i = 0; i < numOfDice; i++)
            {
                yield return Random.Value.Next(1, sides);
            }
        }

    }
}
