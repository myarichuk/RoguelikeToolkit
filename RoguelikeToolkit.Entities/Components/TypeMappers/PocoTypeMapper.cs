using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using FastMember;

namespace RoguelikeToolkit.Entities.Components.TypeMappers
{
    public class PocoTypeMapper : ITypeMapper
    {
        private static readonly ConcurrentDictionary<Type, Dictionary<string, Member>> MembersCacheByType = new();

        public int Priority => int.MaxValue;

        public bool CanMap(Type destType, IReadOnlyDictionary<string, object> data) =>
            !destType.IsCOMObject && !destType.IsPointer && !destType.IsAbstract &&
            !destType.GetInterfaces()
                .Any(type => 
                    type.IsValueComponent() &&
                    type.GenericTypeArguments[0].IsDictionary());

        public object Map(IReadOnlyList<IPropertyMapper> propertyMappers, Type destType, IReadOnlyDictionary<string, object> data, Func<IReadOnlyDictionary<string, object>, Type, object> createInstance, EntityFactoryOptions options = null)
        {
            options ??= EntityFactoryOptions.Default;

            var accessor = MemberAccessor.Get(destType);
            var instance = destType.CreateInstance();

            foreach (var prop in data)
            {
                var member = GetMemberByName(destType, prop.Key);
                if (member == null)
                {
                    if (options.IgnoreMissingFields)
                        continue;
                    else
                        throw new InvalidOperationException($"Tried to find the field {prop.Key} but failed (parent type = {destType.FullName})");
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
                    foreach (var mapper in propertyMappers)
                    {
                        if (mapper.CanMap(member.Type, prop.Value))
                        {
                            accessor[instance, prop.Key] = mapper.Map(propertyMappers, member.Type, prop.Value);
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
                var typeMembers = MembersCacheByType.GetOrAdd(destType,
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
