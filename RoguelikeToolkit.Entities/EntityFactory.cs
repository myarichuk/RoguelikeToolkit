using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using DefaultEcs;
using Utf8Json;

namespace RoguelikeToolkit.Entities
{
    public class EntityFactory
    {
        private readonly World _world;
        private readonly EntityTemplateCollection _templateCollection;
        private readonly Dictionary<string, Type> _components;
        private static readonly MethodInfo EntitySetMethodInfo;

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
                entity.SetAsChildOf(parentEntity.Value);

            foreach (var component in template.Components)
            {
                if(!_components.TryGetValue(component.Key, out var componentType))
                    continue;

                object componentInstance;
                if (componentType.GetInterfaces().Any(@interface => @interface.Name.StartsWith("IValueComponent")))
                {
                    componentInstance = FormatterServices.GetUninitializedObject(componentType);
                    ((dynamic) componentInstance).Value = Convert.ChangeType(component.Value, ((dynamic) componentInstance).Value.GetType());
                }
                else
                {
                    //TODO: consider refactoring this part for cases where type of field in template json mismatches the type defined in component class/struct
                    //Ideally, this will try to convert between types (double can be converted to int for example) or ignore types that cannot be converted
                    //(for now, if Attributes have "int" field but in JSON template its default is defined as "double", this part will throw InvalidDataException
                    try
                    {
                        componentInstance = JsonSerializer.NonGeneric.Deserialize(componentType,
                            JsonSerializer.Serialize(component.Value));
                    }
                    catch (JsonParsingException e)
                    {
                        throw new InvalidDataException($"Failed to instantiate component instance of type '{componentType.FullName}'. Perhaps field types of the component don't match the defaults defined in the template?", e);
                    }
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
