using FastMember;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Utf8Json;

namespace RoguelikeToolkit.Entities
{
    public class ComponentTemplate
    {
        private readonly IReadOnlyDictionary<string, object> _propertyValues;
        private readonly static ConcurrentDictionary<Type, Dictionary<string, MemberInfo>> _membersCache = new ConcurrentDictionary<Type, Dictionary<string, MemberInfo>>();

        internal ComponentTemplate(IReadOnlyDictionary<string, object> propertyValues) => 
            _propertyValues = InitializeEmbeddedTemplates(propertyValues);

        private static IReadOnlyDictionary<string, object> InitializeEmbeddedTemplates(IReadOnlyDictionary<string, object> data)
        {
            var dataWithEmbeddedTemplates = new Dictionary<string, object>();
            foreach (var item in data)
            {
                switch (item.Value)
                {
                    case IReadOnlyDictionary<string, object> embedded:
                        dataWithEmbeddedTemplates.Add(item.Key, new ComponentTemplate(embedded));
                        break;
                    default:
                        dataWithEmbeddedTemplates.Add(item.Key, item.Value);
                        break;
                }
            }
            return dataWithEmbeddedTemplates;
        }

        public static ComponentTemplate Parse(string json)
        {
            var data = (IReadOnlyDictionary<string, object>)JsonSerializer.Deserialize<dynamic>(json);
            return new ComponentTemplate(data);
        }

        public object CreateNew(Type type)
        {
            var typeAccessor = MemberAccessor.Get(type);
            var instance = typeAccessor.CreateNew();
            ApplyPropertyValues(instance, typeAccessor);

            return instance;
        }

        private void ApplyPropertyValues(object instance, TypeAccessor typeAccessor)
        {
            var type = instance.GetType();
            var membersByName = _membersCache.GetOrAdd(type, 
                newType => 
                    newType.GetMembers()
                           .Where(m => m.MemberType == MemberTypes.Field || m.MemberType == MemberTypes.Property)
                           .ToDictionary(m => m.Name, m => m));

            foreach (var prop in _propertyValues)
            {
                if (!membersByName.TryGetValue(prop.Key, out var member))
                    throw new InvalidDataException($"Field or property by name of {prop.Key} wasn't found in the type. This is probably a misuse or you are using an incorrect type");
                var memberType = member.GetUnderlyingType();

                typeAccessor[instance, prop.Key] = 
                    prop.Value switch
                    {
                        ComponentTemplate template => template.CreateNew(memberType),
                        _ => Convert.ChangeType(prop.Value, memberType),
                    };
            }
        }
    }
}
