namespace RoguelikeToolkit.Common.Components
{
    [Component]
    public struct MaterialComponent
    {
        /// <summary>
        /// How much damage reduction should be applied when damaging the material -
        /// Thus DamageResistance = 0.5 would make only half of incoming damage affect the object
        /// </summary>
        public double DamageResistance { get; set; }

        /// <summary>
        /// In real world, physical materials have density. This would serve as weight 'multiplier' - 
        /// material twice dense would have Density = 2.0, therefore final weight would be 'Weight * Density'
        /// </summary>
        public double Density { get; set; }
    }
}
