using System;
using RoguelikeToolkit.DiceExpression;

namespace RoguelikeToolkit.DiceTests
{
	public class DiceEpxpression
	{
		[Theory]
		[InlineData("3d6 * 3 + 2d4")]
		public void Can_parse_dice_expression(string diceExpr)
        {
			var dice = Dice.Parse(diceExpr);
			var diceAst = (string)dice;

			Assert.NotEmpty(diceAst);
        }

		[Theory]
		[InlineData("3d6", 8, new [] {3,2,3})]
		[InlineData("d6", 3, new[] { 3 })]
		[InlineData("2d4 + 3", 8, new[] { 3, 2 })]
		[InlineData("3d6 + 2d4", 10, new[] { 3, 2, 3, 1,1 })]
		[InlineData("3d6 * 3", 24, new[] { 3, 2, 3 })]
		[InlineData("3d6 * (3 + 2d4)", 40, new[] { 3, 2, 3, 1, 1 })]
		[InlineData("3d6 * 3 + 2d4", 26, new[] { 3, 2, 3, 1, 1 })]
		[InlineData("(3d6 * 3) + 2d4", 26, new[] { 3, 2, 3, 1, 1 })]
		public void Can_evaluate_dice_expression(string diceExpr, int expectedResult, int[] ramdomResults)
        {
			try
			{
				Dice.SetRandom(new DeterministicRandom(ramdomResults));
				var dice = Dice.Parse(diceExpr);
				Assert.Equal(expectedResult, dice.Roll());
			}
			finally
			{
				Dice.SetDefaultRandom();
			}
		}
	}
}

