﻿using FastMember;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace RoguelikeToolkit.Entities.Components.TypeMappers
{
    public class PocoTypeMapper : ITypeMapper
    {
        private static readonly Lazy<IReadOnlyList<IPropertyMapper>> _propertyMappers = new(() => Mappers.Instance.PropertyMappers.OrderBy(x => x.Priority).ToList());
        private static readonly ConcurrentDictionary<Type, Dictionary<string, Member>> _membersCacheByType = new();

        public int Priority => int.MaxValue;

        public bool CanMap(Type destType, IReadOnlyDictionary<string, object> data) =>
            !destType.IsCOMObject && !destType.IsPointer && !destType.IsAbstract &&
            !destType.GetInterfaces()
                .Any(i => i.FullName.StartsWith("RoguelikeToolkit.Entities.IValueComponent") &&
                          i.IsGenericType &&
                          i.GenericTypeArguments[0].GetInterfaces()
                                .Any(ii => ii.FullName.StartsWith("System.Collections.Generic.IDictionary")));

        public object Map(Type destType, IReadOnlyDictionary<string, object> data, Func<IReadOnlyDictionary<string, object>, Type, object> createInstance)
        {
            var accessor = MemberAccessor.Get(destType);
            var instance = accessor.CreateNewSupported ? accessor.CreateNew() : FormatterServices.GetUninitializedObject(destType);

            foreach (var prop in data)
            {
                var member = GetMemberByName(destType, prop.Key);
                if (member == null)
                {
                    continue;
                }

                if (prop.Value is ComponentTemplate emdedded)
                {
                    accessor[instance, prop.Key] = createInstance(emdedded.PropertyValues, member.Type);
                }
                else if (prop.Value == default)
                {
                    accessor[instance, prop.Key] = default;
                }
                else
                {
                    bool wasMapped = false;
                    foreach (var mapper in _propertyMappers.Value)
                    {
                        if (mapper.CanMap(member.Type, prop.Value))
                        {
                            accessor[instance, prop.Key] = mapper.Map(member.Type, prop.Value);
                            wasMapped = true;
                            break;
                        }
                    }

                    if (wasMapped == false)
                    {
                        throw new InvalidOperationException($"Couldn't convert property name = {prop.Key} and value = {prop.Value} to type {member.Type.FullName} - no suitable mappers found. This is most likely a bug and should be reported. (final type that needed to be converted = {destType.FullName}");
                    }
                }
            }

            return instance;

            static Member GetMemberByName(Type destType, string propName)
            {
                var typeMembers = _membersCacheByType.GetOrAdd(destType,
                                        type => MemberAccessor.Get(destType)
                                                              .GetMembers()
                                                              .ToDictionary(x => x.Name, x => x));

                if (!typeMembers.TryGetValue(propName, out var member))
                {
                    return null;
                }

                return member;
            }
        }
    }
}
