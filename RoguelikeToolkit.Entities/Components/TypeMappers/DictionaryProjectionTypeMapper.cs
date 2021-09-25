using System;
using System.Collections.Generic;
using System.Linq;

namespace RoguelikeToolkit.Entities.Components.TypeMappers
{
    public class DictionaryProjectionTypeMapper : ITypeMapper
    {
        public int Priority => 2;

        public bool CanMap(Type destType, IReadOnlyDictionary<string, object> data) =>
            destType.GetInterfaces()
                    .Any(type => 
                        type.IsValueComponent() && 
                        type.GenericTypeArguments[0].IsDictionary());

        public object Map(IReadOnlyList<IPropertyMapper> propertyMappers, Type destType, IReadOnlyDictionary<string, object> data, Func<IReadOnlyDictionary<string, object>, Type, object> createInstance, Type[] componentTypes, EntityFactoryOptions options = null)
        {
            var accessor = MemberAccessor.Get(destType);
            var instance = destType.CreateInstance();

            if (((dynamic)instance).Value == null)
            {
                var member = accessor.GetMembers().FirstOrDefault(m => m.Name == "Value");
                if (member == null)
                {
                    throw new InvalidOperationException($"In the type {destType.FullName} I expected to find the property 'Value' but didn't find it. This is not supposed to happen in this case and isclearly some sort of a bug.");
                }

                ((dynamic)instance).Value = (dynamic)member.Type.CreateInstance();
            }

            if (data.Count > 0)
            {
                var dict = ((dynamic)instance).Value; //we know it is IValueComponent, so...

                var itemType = (Type)dict.GetType().GenericTypeArguments[1];
                var converter = propertyMappers.FirstOrDefault(c => c.CanMap(itemType, data.Values.First()));
                if (converter == null)
                {
                    throw new InvalidOperationException($"Cannot map between collections, couldn't find appropriate mapper between {dict.GetType().GenericTypeArguments[1].FullName} and {data.Values.First().GetType().FullName}");
                }

                foreach (var prop in data)
                {
                    dict.Add(prop.Key, (dynamic)converter.Map(propertyMappers, itemType, prop.Value, componentTypes));
                }
            }
            return instance;
        }
    }
}
