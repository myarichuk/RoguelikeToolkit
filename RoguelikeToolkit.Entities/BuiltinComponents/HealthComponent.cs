namespace RoguelikeToolkit.Entities.BuiltinComponents
{
    [Component(Name = "Health")]
    public struct HealthComponent : IValueComponent<double>
    {
        public double Value { get; set; }
    }
}
