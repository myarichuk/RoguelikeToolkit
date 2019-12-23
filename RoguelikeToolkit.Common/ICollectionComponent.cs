using System.Collections.Generic;

namespace RoguelikeToolkit.Common
{
    public interface ICollectionComponent<T>
    {
        ICollection<T> Values { get; }
    }
}
