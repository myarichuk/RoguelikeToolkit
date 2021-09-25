using System;
using System.Collections.Generic;
using System.Dynamic;

namespace RoguelikeToolkit.Entities.Components.TypeMappers
{
    public class DynamicTypeMapper : ITypeMapper
    {
        public int Priority => 0;

        public bool CanMap(Type destType, IReadOnlyDictionary<string, object> data) =>
            destType == typeof(object);

        public object Map(IReadOnlyList<IPropertyMapper> propertyMappers, Type destType, IReadOnlyDictionary<string, object> data, Func<IReadOnlyDictionary<string, object>, Type, object> createInstance, Type[] componentTypes, EntityFactoryOptions options = null)
        {
            var instance = (IDictionary<string, object>)new ExpandoObject();

            foreach (var prop in data)
            {
                if (prop.Value == default)
                {
                    instance.Add(prop.Key, default);
                }
                else if (prop.Value is ComponentTemplate embedded)
                {
                    instance.Add(prop.Key, createInstance(embedded.PropertyValues, typeof(object)));
                }
                else if (prop.Value is IDictionary<string, object> embeddedDict)
                {
                    instance.Add(prop.Key, embeddedDict.ToExpando());
                }
                else
                {
                    instance.Add(prop.Key, prop.Value);
                }
            }

            return instance;
        }
    }
}
