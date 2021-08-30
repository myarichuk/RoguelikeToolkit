using RoguelikeToolkit.Entities.Components.TypeMappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoguelikeToolkit.Entities.Components
{
    internal class ComponentInstanceCreator
    {
        private static readonly Lazy<IReadOnlyList<ITypeMapper>> _typeMappers = new(() => Mappers.Instance.TypeMappers.OrderBy(x => x.Priority).ToList());

        public object CreateInstance(Type type, IReadOnlyDictionary<string, object> data)
        {
            object instance = null;
            bool wasMapped = false;
            foreach (var mapper in _typeMappers.Value)
            {
                if (mapper.CanMap(type, data))
                {
                    instance = mapper.Map(type, data, (innerData, innerType) => CreateInstance(innerType, innerData));
                    wasMapped = true;
                    break;
                }
            }

            if (wasMapped == false)
                throw new InvalidOperationException($"Failed to create component instance of type {type.FullName}, no suitable mappers found");

            return instance;
        }
    }
}
