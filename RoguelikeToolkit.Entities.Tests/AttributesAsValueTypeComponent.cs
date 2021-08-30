using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace RoguelikeToolkit.Entities.Tests
{
    public class AttributesAsValueTypeComponent : IValueComponent<Dictionary<string, int>>
    {
        public Dictionary<string, int> Value { get; set; }
    }

    public class AttributesWithEnumAsValueTypeComponent : IValueComponent<Dictionary<string, KnownColor>>
    {
        public Dictionary<string, KnownColor> Value { get; set; }
    }
}
