using System.Collections.Generic;
using RoguelikeToolkit.Common;

namespace RoguelikeToolkit.Tryouts
{
    public class CapabilitiesComponent : IArrayComponent<string>
    {
        private readonly List<string> _values = new List<string>();
        public ICollection<string> Values => _values;

    }
}
