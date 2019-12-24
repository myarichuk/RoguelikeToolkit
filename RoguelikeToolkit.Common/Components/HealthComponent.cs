namespace RoguelikeToolkit.Common.Components
{
    /// <summary>
    /// Represents in percents, how 'whole' is the object or how healthy biological object/creature is
    /// Useful to determine when object get broken/destroyed or crippled/dead (in case of biological nature of the object)
    /// </summary>
    public struct HealthComponent : IValueComponent<double>
    {
        //TODO: don't forget to implement 'rust' mechanics for metallic equipment 
        public double Value { get; set; }
    }
}
