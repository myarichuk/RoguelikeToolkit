using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using DefaultEcs;
using DefaultEcs.System;
using FastMember;
using RoguelikeToolkit.Common.EntityTemplates;

namespace RoguelikeToolkit.Common.Entities
{
    public class EntityFactory
    {
        private static readonly Dictionary<string, Type> ComponentTypes;
        private static readonly Dictionary<Type, TypeAccessor> TypeAccessorCache = new Dictionary<Type, TypeAccessor>();
        private static readonly MethodInfo EntitySetMethodInfo;
        private static readonly Dictionary<Type, MethodInfo> EntitySetMethodCache = new Dictionary<Type, MethodInfo>();
        static EntityFactory()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            var components = 
                (from assembly in assemblies
                 from type in assembly.GetTypes()
                 where type.Name.EndsWith("component", true, CultureInfo.InvariantCulture) ||
                       type.GetInterfaces()
                           .Any(it => it.IsGenericType && it.GetGenericTypeDefinition() == typeof(IValueComponent<>)) ||
                       type.GetInterfaces()
                           .Any(it => it.IsGenericType && it.GetGenericTypeDefinition() == typeof(ICollectionComponent<>)) ||
                       type.GetCustomAttributes(typeof(ComponentAttribute), true).Any()
                 select type).ToArray();

            ComponentTypes = components.ToDictionary(x => 
                x.Name.EndsWith("component", true, CultureInfo.InvariantCulture) ? 
                    x.Name.Replace("component", string.Empty, true, CultureInfo.InvariantCulture) : 
                    x.Name, 
                x => x);

            EntitySetMethodInfo = typeof(Entity).GetMethod(nameof(Entity.Set));
        }

        private readonly EntityTemplateRepository _templateRepository;

        public EntityFactory(EntityTemplateRepository templateRepository) => 
            _templateRepository = templateRepository;

        private class ComponentsVisitorState
        {
            public Entity Current { get; set; }
        }

        public bool TryCreate(string templateId, World world, ref Entity entity)
        {
            if (!_templateRepository.Templates.ContainsKey(templateId)) 
                return false;

            var template = _templateRepository.Templates[templateId];

            VisitTemplateHierarchy(template, ref entity);

            void VisitTemplateHierarchy(EntityTemplate current, ref Entity parent)
            {
                ApplyComponents(current, ref parent);

                if (current.Children.Count <= 0) 
                    return;

                foreach (var kvp in current.Children)
                {
                    var childEntity = world.CreateEntity();

                    childEntity.SetAsChildOf(in parent);
                    parent.SetAsParentOf(in childEntity);

                    VisitTemplateHierarchy(kvp.Value, ref childEntity);
                }
            }

            //TODO: finish here

            return true;

        }

        private static readonly object[] Parameters = new object[1];

        private void ApplyComponents(EntityTemplate current, ref Entity entity)
        {
            bool HasInterface(object obj, Type interfaceType)
            {
                IEnumerable<Type> interfaces = obj.GetType().GetInterfaces();
                if (interfaceType.IsGenericType) interfaces = interfaces.Where(i => i.IsGenericType);

                return interfaces.Any(x => (interfaceType.IsGenericType ? x.GetGenericTypeDefinition() : x) == interfaceType);
            }

            object Str2Enum(string str, Type enumType)
            {
                try
                {
                    var res = Enum.Parse(enumType, str);
                    if (!Enum.IsDefined(enumType, res)) return default;
                    return res;
                }
                catch
                {
                    return default;
                }
            }

            foreach (var componentData in current.Components)
            {
                if (!ComponentTypes.ContainsKey(componentData.Key)) continue;

                var componentType = ComponentTypes[componentData.Key];
                var componentInstance = FormatterServices.GetUninitializedObject(componentType);

                if (!TypeAccessorCache.TryGetValue(componentType, out var typeAccessor))
                {
                    typeAccessor = TypeAccessor.Create(componentType, true);
                    TypeAccessorCache.Add(componentType, typeAccessor);
                }

                //TODO: handle here both primitives and nested objects
                switch (componentData.Value)
                {
                    case Dictionary<string, object> componentDataValues:
                        foreach (var (key, value) in componentDataValues)
                        {
                            try
                            {
                                var memberInfo = componentType.GetMember(key,MemberTypes.Property | MemberTypes.Field, BindingFlags.Public | BindingFlags.Instance).FirstOrDefault();
                                if(memberInfo == null) //not found, nothing to do
                                    continue;
                                Type memberType;
                                switch (memberInfo.MemberType)
                                {
                                    case MemberTypes.Property:
                                        memberType = ((PropertyInfo) memberInfo).PropertyType;
                                        break;
                                    case MemberTypes.Field:
                                        memberType = ((FieldInfo) memberInfo).FieldType;
                                        break;
                                    default:
                                        throw new InvalidOperationException($"This should never happen. Expected '{key}' to be either field or property but it is '{memberInfo.MemberType}'"); 
                                }

                                if (memberType.IsEnum && value is string str)
                                {
                                    typeAccessor[componentInstance, key] = Str2Enum(str, memberType);
                                }
                                else
                                {
                                    typeAccessor[componentInstance, key] = Convert.ChangeType(value, memberType);
                                }
                            }
                            catch (Exception e) when (e is ArgumentOutOfRangeException || e is InvalidCastException)
                            {
                                //don't error on non-existing properties
                                //TODO: add logging
                            }
                        }

                        break;
                    case IEnumerable collection when (collection is string): //string is also IEnumerable
                        goto default;
                    case IEnumerable collection:
                        //precaution, TODO: logging if doesn't implement 
                        if (!HasInterface(componentInstance, typeof(ICollectionComponent<>))) break;

                        var dynamicCollectionComponentInstance = (dynamic) componentInstance;

                        var collectionInterface = componentType.GetInterfaces()
                                                    .First(iv => iv.IsGenericType && iv.GetGenericTypeDefinition() == typeof(ICollectionComponent<>));
                        var collectionItemType = collectionInterface.GetGenericArguments()[0];
                        var collectionType = typeof(List<>).MakeGenericType(collectionItemType);
                        //TODO: use reflection libraries that make Activator.CreateInstance faster
                        if (dynamicCollectionComponentInstance.Values == null)
                            dynamicCollectionComponentInstance.Values = (dynamic)Activator.CreateInstance(collectionType);
                        foreach (var item in collection) dynamicCollectionComponentInstance.Values.Add((dynamic) item);

                        break;
                    default: //primitives and string...
                        //precaution, TODO: logging if doesn't implement 
                        //we want to ensure - components that contain ONE primitive like string or a number
                        // - they HAVE to implement IValueComponent
                        if (!HasInterface(componentInstance, typeof(IValueComponent<>))) break;

                        var dynamicValueComponentInstance = (dynamic) componentInstance;
                        var @interface = componentType.GetInterfaces().First(iv => iv.IsGenericType && iv.GetGenericTypeDefinition() == typeof(IValueComponent<>));

                        dynamicValueComponentInstance.Value = Convert.ChangeType(componentData.Value, @interface.GetGenericArguments()[0]);

                        break;
                }

                if(!EntitySetMethodCache.ContainsKey(componentType))
                    EntitySetMethodCache.Add(componentType, EntitySetMethodInfo.MakeGenericMethod(componentType));

                var setMethod = EntitySetMethodCache[componentType];
                Parameters[0] = componentInstance;
                setMethod.Invoke(entity, Parameters);
            }
        }
    }
}
