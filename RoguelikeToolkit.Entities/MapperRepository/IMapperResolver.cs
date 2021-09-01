using System;
using System.Collections.Generic;

namespace RoguelikeToolkit.Entities
{
    public interface IMapperResolver
    {
        IEnumerable<Type> GetTypeMappers();

        IEnumerable<Type> GetPropertyMappers();
    }
}
