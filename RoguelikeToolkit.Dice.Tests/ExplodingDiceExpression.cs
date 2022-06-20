using System;
using RandN;
using RandN.Compat;
using RoguelikeToolkit.DiceExpression;

namespace RoguelikeToolkit.DiceTests
{
    internal class DeterministicRandom : Random
    {
        private readonly Random _default = RandomShim.Create(SmallRng.Create());
        private readonly IReadOnlyList<int> _deterministicValues;
        private int _currentIndex = 0;

        public DeterministicRandom(IReadOnlyList<int> deterministicValues) =>
            _deterministicValues = deterministicValues;

        public override int Next(int minValue, int maxValue)
        {
            if (_currentIndex < _deterministicValues.Count)
                return _deterministicValues[_currentIndex++];
            
            return _default.Next(minValue, maxValue);
        }
    }

    public class ExplodingDiceExpression
	{

		[Theory]
		[InlineData("3d6!",  15,  new[] { 6, 6, 1, 1, 1, 1 })]
        [InlineData("3d6!3",  9,  new[] { 6, 2, 1, 3, 4, 5 })]
        [InlineData("3d6!3",  9,  new[] { 3, 3, 1, 1, 1, 1 })]
        [InlineData("3d6!12", 7,  new[] { 3, 3, 1, 1, 1, 1 })]
        public void Can_evaluate_exploding_dice_expression(string diceExpr, int expectedResult, int[] ramdomResults)
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

