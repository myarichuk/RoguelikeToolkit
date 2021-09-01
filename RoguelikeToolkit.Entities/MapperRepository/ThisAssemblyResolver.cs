using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using RoguelikeToolkit.Entities.Components.TypeMappers;

namespace RoguelikeToolkit.Entities
{
    public class ThisAssemblyResolver : IMapperResolver
    {
        private readonly Assembly _thisAssembly = Assembly.GetExecutingAssembly();

        public IEnumerable<Type> GetPropertyMappers() =>
            _thisAssembly.GetTypes().Where(t => t.ImplementsInterface<IPropertyMapper>());

        public IEnumerable<Type> GetTypeMappers() =>
            _thisAssembly.GetTypes().Where(t => t.ImplementsInterface<ITypeMapper>());
    }
}
