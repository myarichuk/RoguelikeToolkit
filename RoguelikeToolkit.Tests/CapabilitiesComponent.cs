using System.Collections.Generic;
using RoguelikeToolkit.Common;

namespace RoguelikeToolkit.Tests
{
    public class CapabilitiesComponent : ICollectionComponent<string>
    {
        public ICollection<string> Values { get; set; }
    }
}
