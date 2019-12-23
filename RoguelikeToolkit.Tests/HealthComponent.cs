using RoguelikeToolkit.Common;

namespace RoguelikeToolkit.Tryouts
{
    public struct HealthComponent : IValueComponent<double>
    {
        public double Value { get; set; }
    }
}
