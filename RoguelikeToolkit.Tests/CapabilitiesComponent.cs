using System.Collections.Generic;
using RoguelikeToolkit.Common;

namespace RoguelikeToolkit.Tryouts
{
    public class CapabilitiesComponent : ICollectionComponent<string>
    {
        public ICollection<string> Values { get; set; }
    }
}
