using System.Drawing;

namespace RoguelikeToolkit.Entities.Tests
{
    public struct AttributeAsEnumComponent : IValueComponent<KnownColor>
    {
        public KnownColor Value {  get; set; }
    }
}
