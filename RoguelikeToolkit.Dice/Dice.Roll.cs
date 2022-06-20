using RandN;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using RandN.Compat;
using System;

namespace RoguelikeToolkit.DiceExpression
{
	public partial class Dice
	{
		//NR3Q1Generator is not thread-safe
		private static ThreadLocal<Random> Random =
			new(() => RandomShim.Create(SmallRng.Create() as IRng));

		public static void SetRandom(Random randomImpl) =>
			Random.Value = randomImpl;

		public static void SetDefaultRandom() =>
			Random.Value = RandomShim.Create(SmallRng.Create() as IRng);

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

#nullable enable
		internal static int RollExploding(int numOfDice, int sides, Func<int, bool>? explodeCondition = null)
#nullable disable
		{
			var rolledDiceCount = numOfDice;
			var result = 0;
			explodeCondition ??= num => num == sides;

			while (rolledDiceCount > 0)
			{
				var currentResult = Random.Value.Next(1, sides);
				result += currentResult;
				while (explodeCondition(currentResult))
				{
					currentResult = Random.Value.Next(1, sides);
					result += currentResult;
				}
				rolledDiceCount--;
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
