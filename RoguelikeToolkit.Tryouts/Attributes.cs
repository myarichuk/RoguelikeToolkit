using RoguelikeToolkit.Entities;

namespace RoguelikeToolkit.Tryouts
{
    [Component(Name = "Attributes")]
    public class Attributes
    {
        public int Strength { get; set; }
#pragma warning disable S1104 // Fields should not have public accessibility
        public int Agility;
#pragma warning restore S1104 // Fields should not have public accessibility
    }
}
