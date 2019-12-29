using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace RoguelikeToolkit.Entities
{
    public class EntityFactory
    {
        private readonly EntityTemplateCollection _templateCollection;
        private readonly Dictionary<string, Type> _components;

        public Func<Type, string> ComponentNameExtractor { get; set; }

        public EntityFactory(EntityTemplateCollection templateCollection,
            params Assembly[] componentAssemblies)
            : this(templateCollection, DefaultComponentNameExtractor, componentAssemblies)
        {
        }

        public EntityFactory(EntityTemplateCollection templateCollection, Func<Type, string> componentNameExtractor, params Assembly[] componentAssemblies)
        {
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

        public bool TryRegisterComponent<T>()
        {
            var type = typeof(T);
            var key = ComponentNameExtractor(type);

            #if NETSTANDARD2_1
            return _components.TryAdd(key, type);
            #else
            if (_components.ContainsKey(key))
                return false;
            _components.Add(key, type);
            return true;
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
