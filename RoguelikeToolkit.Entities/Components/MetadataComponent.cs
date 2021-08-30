using System.Collections.Generic;

namespace RoguelikeToolkit.Entities
{ 
    public struct MetadataComponent : IValueComponent<HashSet<string>>
    {
        public HashSet<string> Value { get; set; }
    }
}
