using System;

namespace RoguelikeToolkit.Common
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class ComponentAttribute : Attribute
    {
        public string Name { get; set; }
    }
}
