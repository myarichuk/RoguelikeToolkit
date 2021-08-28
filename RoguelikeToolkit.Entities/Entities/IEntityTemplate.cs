using System.Collections.Generic;

namespace RoguelikeToolkit.Entities
{
    public interface IEntityTemplate
    {
        Dictionary<string, ComponentTemplate> Components { get; }
        string Id { get; }
        HashSet<string> Inherits { get; }
        HashSet<string> Tags { get; }
    }
}