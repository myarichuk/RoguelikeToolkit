using System;
using System.Collections.Generic;
using System.Text;

namespace RoguelikeToolkit.Entities.Tests
{
    public class AttributesAsValueTypeComponent : IValueComponent<Dictionary<string, int>>
    {
        public Dictionary<string, int> Value { get; set; }
    }
}
