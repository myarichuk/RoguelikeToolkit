using RoguelikeToolkit.DiceExpression;
using System.Data;

namespace RoguelikeToolkit.DiceTests
{
	public class Basics
	{

		[Theory]
		[InlineData("3 + 4", 7)]
		[InlineData("10 - 6", 4)]
		[InlineData("3 * 4", 12)]
		[InlineData("3 * 4 + 3", 15)]
		[InlineData("3 * 4 - 2", 10)]
		[InlineData("4 / 2 - 2", 0)]
		public void Can_do_arithmetic_operations(string diceExpr, int expectedResult)
		{
            var dice = Dice.Parse(diceExpr);
			Assert.Equal(expectedResult, dice.Roll());
		}

		[Fact]
		public void Should_throw_proper_error_on_divide_by_zero()
		{
			var dice = Dice.Parse("3 / 0", true);
			var ex = Assert.Throws<DiceRollFailedException>(() => dice.Roll());
			Assert.IsType<DivideByZeroException>(ex.InnerException);
		}

		[Theory]
		[InlineData("3 & 6")]
		[InlineData("3 + 5 & 6")]
		[InlineData("(3 + 5) = 6")]
		[InlineData("(3 + 5")]
		[InlineData("3 + ")]
		[InlineData("3d")]
		public void Should_throw_syntax_errors(string diceExpr) =>
			Assert.Throws<SyntaxErrorException>(() => Dice.Parse(diceExpr, true));
	}
}
