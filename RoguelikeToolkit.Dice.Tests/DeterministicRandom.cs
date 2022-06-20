using RandN;
using RandN.Compat;

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
}

