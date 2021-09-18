using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RoguelikeToolkit.Entities
{
    public class ComponentTypeRepository : IDictionary<string, Type>
    {
        private readonly Dictionary<string, Type> _componentTypesByName;
        private readonly Func<Type, string> _componentNameExtractor;

        public ComponentTypeRepository(Func<Type, string> componentNameExtractor, params Assembly[] componentAssemblies)
        {
            _componentNameExtractor = componentNameExtractor;
            _componentTypesByName = componentAssemblies
                .Union(new[] { Assembly.GetExecutingAssembly(), Assembly.GetCallingAssembly() })
                .Union(AppDomain.CurrentDomain.GetAssemblies()
                    .Where(x => !x.IsDynamic && //dynamic assemblies may have issues with reflection...
                                !x.FullName.StartsWith("Microsoft.", StringComparison.InvariantCultureIgnoreCase) &&
                                !x.FullName.StartsWith("System.", StringComparison.InvariantCultureIgnoreCase) &&
                                !x.FullName.StartsWith("Windows.", StringComparison.InvariantCultureIgnoreCase)))
                .SelectMany(x =>
                    x.GetTypes()
                        .Where(t =>
                            t.IsInterface == false &&
                            t.IsEnum == false &&
                            t.IsPointer == false &&
                            t.IsCOMObject == false &&
                            (
                                t.Name.EndsWith("Component", StringComparison.InvariantCultureIgnoreCase) ||
                                t.GetCustomAttributes(typeof(ComponentAttribute), true).Any())
                            )
                     )
                .ToDictionary(x => _componentNameExtractor(x), x => x, StringComparer.InvariantCultureIgnoreCase);
        }

        public Type this[string key] { get => _componentTypesByName[key]; set => _componentTypesByName[key] = value; }

        public ICollection<string> Keys => _componentTypesByName.Keys;

        public ICollection<Type> Values => _componentTypesByName.Values;

        public int Count => _componentTypesByName.Count;

        public bool IsReadOnly => false;

        public void Add(string key, Type value) => _componentTypesByName.Add(key, value);

        public void Add(KeyValuePair<string, Type> item) => _componentTypesByName.Add(item.Key, item.Value);

        public void Clear() => _componentTypesByName.Clear();

        public bool Contains(KeyValuePair<string, Type> item) => _componentTypesByName.Contains(item);

        public bool ContainsKey(string key) => _componentTypesByName.ContainsKey(key);
        public void CopyTo(KeyValuePair<string, Type>[] array, int arrayIndex) => 
            ((ICollection<KeyValuePair<string, Type>>)_componentTypesByName).CopyTo(array, arrayIndex);

        public IEnumerator<KeyValuePair<string, Type>> GetEnumerator() => _componentTypesByName.GetEnumerator();
        
        public bool Remove(string key) => _componentTypesByName.Remove(key);

        public bool Remove(KeyValuePair<string, Type> item) => ((ICollection<KeyValuePair<string, Type>>)_componentTypesByName).Remove(item);

        public bool TryGetValue(string key, out Type value) => _componentTypesByName.TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator() => _componentTypesByName.GetEnumerator();
    }
}
