namespace RoguelikeToolkit.Entities.BuiltinComponents
{
    [Component(Name = "Weight")]
    public struct WeightComponent : IValueComponent<double>
    {
        public double Value { get; set; }
    }
}
