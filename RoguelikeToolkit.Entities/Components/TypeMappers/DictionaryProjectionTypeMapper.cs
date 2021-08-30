using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace RoguelikeToolkit.Entities.Components.TypeMappers
{
    public class DictionaryProjectionTypeMapper : ITypeMapper
    {

        /* Unmerged change from project 'RoguelikeToolkit.Entities (netstandard2.1)'
        Before:
                private readonly static Lazy<List<IPropertyMapper>> _propertyMappers = new(() => Mappers.Instance.PropertyMappers.OrderBy(x => x.Priority).ToList());
        After:
                private static readonly Lazy<List<IPropertyMapper>> _propertyMappers = new(() => Mappers.Instance.PropertyMappers.OrderBy(x => x.Priority).ToList());
        */
        private static readonly Lazy<List<IPropertyMapper>> _propertyMappers = new(() => Mappers.Instance.PropertyMappers.OrderBy(x => x.Priority).ToList());

        public int Priority => 2;

        public bool CanMap(Type destType, IReadOnlyDictionary<string, object> data) =>
            destType.GetInterfaces()
                .Any(i => i.FullName.StartsWith("RoguelikeToolkit.Entities.IValueComponent") &&
                          i.IsGenericType &&
                          i.GenericTypeArguments[0].IsDictionary());

        public object Map(Type destType, IReadOnlyDictionary<string, object> data, Func<IReadOnlyDictionary<string, object>, Type, object> createInstance)
        {
            var accessor = MemberAccessor.Get(destType);
            var instance = accessor.CreateNewSupported ? accessor.CreateNew() : FormatterServices.GetUninitializedObject(destType);

            if (((dynamic)instance).Value == null)
            {
                var member = accessor.GetMembers().FirstOrDefault(m => m.Name == "Value");
                if (member == null)
                {
                    throw new InvalidOperationException($"In the type {destType.FullName} I expected to find the property 'Value' but didn't find it. This is not supposed to happen in this case and isclearly some sort of a bug.");
                }

                var dictAccessor = MemberAccessor.Get(member.Type);
                ((dynamic)instance).Value = (dynamic)dictAccessor.CreateNew();
            }

            if (data.Count > 0)
            {
                var dict = ((dynamic)instance).Value; //we know it is IValueComponent, so...

                var itemType = (Type)dict.GetType().GenericTypeArguments[1];
                var converter = _propertyMappers.Value.FirstOrDefault(c => c.CanMap(itemType, data.Values.First()));
                if (converter == null)
                {
                    throw new InvalidOperationException($"Cannot map between collections, couldn't find appropriate mapper between {dict.GetType().GenericTypeArguments[1].FullName} and {data.Values.First().GetType().FullName}");
                }

                foreach (var prop in data)
                {
                    dict.Add(prop.Key, (dynamic)converter.Map(itemType, prop.Value));
                }
            }
            return instance;
        }
    }
}
