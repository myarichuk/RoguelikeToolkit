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
		[InlineData("3d6", 3, 18)]
		[InlineData("d6", 1, 6)]
		[InlineData("2d4 + 3", 5, 11)]
		[InlineData("3d6 + 2d4", 5, 26)]
		[InlineData("3d6 * 3", 9, 54)]
		[InlineData("3d6 * (3 + 2d4)", 15, 198)]
		[InlineData("3d6 * 3 + 2d4", 11, 62)]
		[InlineData("(3d6 * 3) + 2d4", 11, 62)]
		public void Can_evaluate_dice_expression(string diceExpr, int min, int max)
        {
			var dice = Dice.Parse(diceExpr);
			for (int i = 0; i < 10; i++)
			{
				var rollResult = dice.Roll();
				Assert.InRange(rollResult, min, max);
			}
        }
	}
}

