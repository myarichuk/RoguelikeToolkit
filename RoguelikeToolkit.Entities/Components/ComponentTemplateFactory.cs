using FastMember;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace RoguelikeToolkit.Entities
{
    public struct ComponentFactoryOptions
    {
        public bool IgnoreMissingFields;

    }
    public class ComponentFactory
    {
        private readonly static ConcurrentDictionary<Type, Dictionary<string, MemberInfo>> _membersCache = 
            new ConcurrentDictionary<Type, Dictionary<string, MemberInfo>>();
        private readonly ComponentFactoryOptions _options;

        public ComponentFactory(ComponentFactoryOptions options = default) => _options = options;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TInstance CreateInstance<TInstance>(ComponentTemplate template) => (TInstance)CreateInstance(typeof(TInstance), template);

        public object CreateInstance(Type type, ComponentTemplate template)
        {
            if (type.IsInterface) //precaution!
                throw new InvalidOperationException($"Cannot create component instance with interface type. (specified type = {type.FullName})");

            if (type == typeof(string) || type.IsPrimitive || type.IsEnum || type.IsPointer || type.IsCOMObject || type.IsByRef)
                throw new InvalidOperationException($"Cannot create component instance with a specified type. The type should *not* be a primitive, enum, by-ref type or a pointer (specified type = {type.FullName})");

            var typeAccessor = MemberAccessor.Get(type);

            object instance;
            //if true, we are either using dynamics or just plain System.Object
            // (in any case, in this case we shouldn't use an System.Object type)
            if (typeof(object) == type) 
            {
                instance = new ExpandoObject();
                ApplyPropertyValuesForExpando((IDictionary<string, object>)instance, template.PropertyValues);
            }
            else
            {
                instance = FormatterServices.GetUninitializedObject(type);
                ApplyPropertyValuesForConcreteType(type, instance, template.PropertyValues, typeAccessor);
            }
            

            return instance;
        }

        private void ApplyPropertyValuesForExpando(
                                 IDictionary<string, object> instance,
                                 IReadOnlyDictionary<string, object> propertyValues)
        {
            foreach (var prop in propertyValues)
            {
                if(prop.Value is ComponentTemplate embeddedTemplate)
                    instance.Add(prop.Key, CreateInstance<dynamic>(embeddedTemplate));
                else
                    instance.Add(prop.Key, prop.Value ?? default);
            }
        }

        private void ApplyPropertyValuesForConcreteType(Type type,
                                         object instance,
                                         IReadOnlyDictionary<string, object> propertyValues,
                                         TypeAccessor typeAccessor)
        {
            var membersByName = _membersCache.GetOrAdd(type,
                                    newType =>
                                        newType.GetMembers()
                                               .Where(m =>
                                                    m.MemberType == MemberTypes.Field ||
                                                    m.MemberType == MemberTypes.Property)
                                               .ToDictionary(m => m.Name, m => m));

            var valueComponentInterface = type.GetInterfaces().FirstOrDefault(i => i.FullName.Contains("RoguelikeToolkit.Entities.IValueComponent"));
            if (valueComponentInterface != null)
            {
                if (valueComponentInterface.GenericTypeArguments[0].IsInterface ||
                    valueComponentInterface.GenericTypeArguments[0].IsPointer)
                {
                    throw new InvalidOperationException($"Unexpected IValueComponent argument, it should not be an interface or a pointer (but it is {valueComponentInterface.GenericTypeArguments[0].FullName} for the type {type.FullName}) ");
                }

                //special case...
                //note: contains rather ugly code, need to figure out a better way :)
                if(valueComponentInterface.GenericTypeArguments[0].Name == "Dictionary`2")
                {
                    ApplyPropertyValuesToDictionary(type, instance, propertyValues, valueComponentInterface);
                    return;
                }
            }


            foreach (var prop in propertyValues)
            {
                if (!membersByName.TryGetValue(prop.Key, out var member))
                {
                    if (_options.IgnoreMissingFields)
                        continue;

                    throw new InvalidDataException($"Field or property by name of {prop.Key} wasn't found in the type. It *is* possible to have duck-typing, but still at least the names of fields should be the same and types should be convertible (for example, int <-> double)");
                }
                var memberType = member.GetUnderlyingType();

                if (memberType.IsPointer) //we don't care about unmanaged types, this is an edge case!
                    continue;

                if (prop.Value is ComponentTemplate embeddedTemplate)
                    typeAccessor[instance, prop.Key] = CreateInstance(memberType, embeddedTemplate);
                else
                {
                    try
                    {
                        typeAccessor[instance, prop.Key] = prop.Value != null ?
                            Convert.ChangeType(prop.Value, memberType) :
                            default;
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
                }
            }
        }

        private static void ApplyPropertyValuesToDictionary(Type type, object instance, IReadOnlyDictionary<string, object> propertyValues, Type valueComponentInterface)
        {

            ((dynamic)instance).Value = (dynamic)FormatterServices.GetUninitializedObject(valueComponentInterface.GenericTypeArguments[0]);

            var valueType = valueComponentInterface.GenericTypeArguments[0].GenericTypeArguments[1];
            //by now we know that the Value is a dictionary, so...
            var component = ((dynamic)instance).Value;
            foreach (var prop in propertyValues)
            {
                try
                {
                    component.Add(prop.Key, (dynamic)Convert.ChangeType(prop.Value, valueType));
                }
                catch (OverflowException e)
                {
                    throw new InvalidOperationException($"Failed to convert {prop.Value} to {valueType}, this is most likely due to incorrect component type being specified. ", e);
                }
                catch (FormatException e)
                {
                    throw new InvalidOperationException($"Failed to convert {prop.Value} to {valueType}, this is most likely due to weird value format that wasn't recognized. ", e);
                }
                catch (InvalidCastException e)
                {
                    throw new InvalidOperationException($"Failed to convert {prop.Value} to {valueType}, the conversion is most likely not supported. Try implementing IConvertible to solve this...", e);
                }
            }
        }
    }
}
