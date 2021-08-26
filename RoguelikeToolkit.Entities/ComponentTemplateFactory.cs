using FastMember;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace RoguelikeToolkit.Entities
{
    public class ComponentFactory
    {
        private readonly static ConcurrentDictionary<Type, Dictionary<string, MemberInfo>> _membersCache = 
            new ConcurrentDictionary<Type, Dictionary<string, MemberInfo>>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TInstance CreateInstance<TInstance>(ComponentTemplate template) => (TInstance)CreateInstance(typeof(TInstance), template);

        public object CreateInstance(Type type, ComponentTemplate template)
        {
            if (type.IsInterface) //precaution!
                throw new InvalidOperationException($"Invalid specified type. Cannot create component instance with interface type. (specified type = {type.FullName})");

            var typeAccessor = MemberAccessor.Get(type);
            var instance = FormatterServices.GetUninitializedObject(type);

            ApplyPropertyValues(instance, template.PropertyValues, typeAccessor);

            return instance;
        }

        private void ApplyPropertyValues(object instance,
                                         IReadOnlyDictionary<string, object> propertyValues,
                                         TypeAccessor typeAccessor)
        {
            var type = instance.GetType();
            var membersByName = _membersCache.GetOrAdd(type,
                                    newType =>
                                        newType.GetMembers()
                                               .Where(m => 
                                                    m.MemberType == MemberTypes.Field || 
                                                    m.MemberType == MemberTypes.Property)
                                               .ToDictionary(m => m.Name, m => m));

            foreach (var prop in propertyValues)
            {
                if (!membersByName.TryGetValue(prop.Key, out var member))
                    throw new InvalidDataException($"Field or property by name of {prop.Key} wasn't found in the type. It *is* possible to have duck-typing, but still at least the names of fields should be the same and types should be convertible (for example, int <-> double)");
                var memberType = member.GetUnderlyingType();

                if (memberType.IsPointer) //we don't care about unmanaged types, this is an edge case!
                    continue;

                switch (prop.Value)
                {
                    case ComponentTemplate embeddedTemplate:
                        typeAccessor[instance, prop.Key] = CreateInstance(memberType, embeddedTemplate);
                        break;
                    default:
                        {
                            switch (prop.Value)
                            {
                                case null:
                                    typeAccessor[instance, prop.Key] = default;
                                    break;
                                default:
                                    {
                                        try
                                        {
                                            typeAccessor[instance, prop.Key] = Convert.ChangeType(prop.Value, memberType);
                                        }
                                        catch (OverflowException e)
                                        {
                                            throw new InvalidOperationException($"Failed to convert {prop.Value} to {memberType.Name}, this is most likely due to incorrect component type being specified. ", e);
                                        }
                                        catch (FormatException e)
                                        {
                                            throw new InvalidOperationException($"Failed to convert {prop.Value} to {memberType.Name}, this is most likely due to weird value format that wasn't recognized. ", e);
                                        }
                                        catch (InvalidCastException e)
                                        {
                                            throw new InvalidOperationException($"Failed to convert {prop.Value} to {memberType.Name}, the conversion is most likely not supported. Try implementing IConvertible to solve this...", e);
                                        }

                                        break;
                                    }
                            }

                            break;
                        }
                }
            }
        }
    }
}
