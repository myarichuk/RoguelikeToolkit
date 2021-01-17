using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using AutoMapper;
using DefaultEcs;

namespace RoguelikeToolkit.Entities
{
    public class EntityFactory
    {
        private readonly World _world;
        private readonly EntityTemplateCollection _templateCollection;
        private readonly Dictionary<string, Type> _components;
        private static readonly MethodInfo EntitySetMethodInfo;

        private readonly Mapper _mapper = new Mapper(new MapperConfiguration(cfg =>
        {
            cfg.ShouldMapProperty = p => p.GetMethod.IsPublic || p.GetMethod.IsAssembly;
        }));

        static EntityFactory()
        {
            var entityType = typeof(Entity);
             EntitySetMethodInfo = entityType.GetMethod("Set", BindingFlags.Instance | BindingFlags.Public);
        }

        public Func<Type, string> ComponentNameExtractor { get; set; }

        public EntityFactory(World world, EntityTemplateCollection templateCollection,
            params Assembly[] componentAssemblies)
            : this(world, templateCollection, DefaultComponentNameExtractor, componentAssemblies)
        {
        }

        public EntityFactory(World world, EntityTemplateCollection templateCollection, Func<Type, string> componentNameExtractor, params Assembly[] componentAssemblies)
        {
            _world = world;
            _templateCollection = templateCollection;
            ComponentNameExtractor = componentNameExtractor;
            _components = componentAssemblies
                .Union(new []{ Assembly.GetExecutingAssembly(), Assembly.GetCallingAssembly() })
                .Union(AppDomain.CurrentDomain.GetAssemblies()
                    .Where(x => !x.FullName.StartsWith("Microsoft.", StringComparison.InvariantCultureIgnoreCase) &&
                                !x.FullName.StartsWith("System.", StringComparison.InvariantCultureIgnoreCase) &&
                                !x.FullName.StartsWith("Windows.", StringComparison.InvariantCultureIgnoreCase)))
                .SelectMany(x => x.GetTypes().Where(t =>
                    t.Name.EndsWith("Component", StringComparison.InvariantCultureIgnoreCase) ||
                    t.GetCustomAttributes(typeof(ComponentAttribute), true).Any()))
                .ToDictionary(x => ComponentNameExtractor(x), x => x, StringComparer.InvariantCultureIgnoreCase);
        }

        public bool TryCreateEntity(string typeName, out Entity result)
        {
            result = default;
            if(!_templateCollection.LoadedTemplates.TryGetValue(typeName, out var template))
                return false;

            result = _world.CreateEntity();
            InstantiateTemplate(template, result, null);

            return true;
        }

        //TODO: think of generic interface to build ECS entity from the template-instantiated data
        private void InstantiateTemplate(EntityTemplate template, Entity entity, Entity? parentEntity)
        {
            if (parentEntity.HasValue)
            {
                entity.SetAsChildOf(parentEntity.Value);
            }

            foreach (var component in template.Components)
            {
                if(!_components.TryGetValue(component.Key, out var componentType))
                    continue;

                object componentInstance;
                if (componentType.GetInterfaces().Any(@interface => @interface.Name.StartsWith("IValueComponent")))
                {
                    componentInstance = FormatterServices.GetUninitializedObject(componentType);
                    var propInfo = componentType.GetProperty("Value");
                    if(propInfo == null)
                        throw new InvalidOperationException($"Tried to get Value property from {componentType.Name} but failed. This is not something that is supposed to happen and should be reported in the Github repo as an issue.");

                    var propertyType = propInfo.PropertyType;
                    if (propertyType.IsClass)
                    {
                        dynamic dest = Activator.CreateInstance(propertyType);
                        if(!(component.Value is Dictionary<string, object> dict))
                            throw new InvalidDataException(
                                $"Expected deserialized component to be of type Dictionary<string, object>, but it was of type '{componentType.FullName}' - this is probably a bug.");

                        _mapper.Map(dict,dest, typeof(Dictionary<string, object>), propertyType);
                        ((dynamic) componentInstance).Value = Convert.ChangeType(dest, propertyType);
                    }
                    else
                    {
                        componentInstance = FormatterServices.GetUninitializedObject(componentType);
                        ((dynamic) componentInstance).Value = Convert.ChangeType(component.Value,
                            ((dynamic) componentInstance).Value.GetType());
                    }
                }
                else if(component.Value is Dictionary<string, object> dict)
                {
                    componentInstance = FormatterServices.GetUninitializedObject(componentType);
                    _mapper.Map(dict, componentInstance, typeof(Dictionary<string, object>), componentType);
                }
                else
                {
                    throw new InvalidDataException($"Expected deserialized component to be of type Dictionary<string, object>, but it was of type '{componentType.FullName}' - this is probably a bug.");
                }
                var entitySetMethod = EntitySetMethodInfo.MakeGenericMethod(componentType);
                entitySetMethod.Invoke(entity, new[] {componentInstance});

            }

            foreach (var childTemplate in template.Children)
                InstantiateTemplate(childTemplate.Value, _world.CreateEntity(), entity);
        }

        //will do nothing if the type is already registered
        public void RegisterComponent<T>()
        {
            var type = typeof(T);
            var key = ComponentNameExtractor(type);

            #if NETSTANDARD2_1
            _components.TryAdd(key, type);
            #else
            if (!_components.ContainsKey(key))
                _components.Add(key, type);
            #endif
        }

        private static string DefaultComponentNameExtractor(Type componentType)
        {
            string DefaultGetComponentName() => componentType.Name.Replace("Component", string.Empty);

            var componentAttribute = componentType.GetCustomAttribute<ComponentAttribute>();
            return componentAttribute != null ?
                componentAttribute.Name ?? DefaultGetComponentName() :
                DefaultGetComponentName();
        }
    }
}
