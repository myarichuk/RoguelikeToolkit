using System.Collections.Generic;

namespace RoguelikeToolkit.Common
{
    public interface IArrayComponent<T>
    {
        ICollection<T> Values { get; }
    }
}
