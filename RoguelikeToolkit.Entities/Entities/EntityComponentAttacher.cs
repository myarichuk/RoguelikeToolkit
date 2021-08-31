using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DefaultEcs;
using Microsoft.Extensions.ObjectPool;

namespace RoguelikeToolkit.Entities
{
    public class EntityComponentAttacher
    {
        private static readonly MethodInfo EntitySetMethodInfo;
        private readonly Func<Type, string> ComponentNameExtractor;
        private readonly ComponentFactory _componentFactory = new();
        private readonly Dictionary<string, Type> _componentTypesByName;
        private readonly Dictionary<Type, MethodInfo> _entitySetByTypeCache = new();
        private readonly ObjectPool<object[]> _paramArrayPool = ObjectPoolProvider.Instance.Create<object[]>(new ReflectionParamsPooledObjectPolicy<object>(1));

        static EntityComponentAttacher()
        {
            var entityType = typeof(Entity);
            var methods = entityType.GetMethods(BindingFlags.Instance | BindingFlags.Public);
            EntitySetMethodInfo = methods.FirstOrDefault(m => m.GetParameters().Length == 1 && m.GetParameters()[0].IsIn);

            //check just in case, should never happen
            if (EntitySetMethodInfo == null)
            {
                throw new ApplicationException("Couldn't find appropriate Entity::Set<T>() method. This probably means that DefaultEcs library has been updated with breaking changes. This is definitely not supposed to happen and should be reported :)");
            }
        }

        public EntityComponentAttacher(
            Func<Type, string> componentNameExtractor,
            ComponentFactoryOptions? componentFactoryOptions,
            params Assembly[] componentAssemblies)
        {
            ComponentNameExtractor = componentNameExtractor ?? throw new ArgumentNullException(nameof(componentNameExtractor));
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
                .ToDictionary(x => ComponentNameExtractor(x), x => x, StringComparer.InvariantCultureIgnoreCase);
        }

        public void InstantiateAndAttachComponent(string componentName, ComponentTemplate template, EntityFactoryOptions options, ref Entity entity)
        {
            //TODO: finish integration _options into different property type mappers
            if (template is null) //sanity check
            {
                throw new ArgumentNullException(nameof(template));
            }

            if (!_componentTypesByName.TryGetValue(componentName, out var componentType))
            {
                ThrowFailedToFindComponentType(componentName);
            }

            var componentInstance = _componentFactory.CreateInstance(componentType, options, template);
            var entitySetMethod = GetOrCacheEntitySetMethod(componentType);

            object[] @params = null;
            try
            {
                @params = _paramArrayPool.Get();
                @params[0] = componentInstance;
                entitySetMethod.Invoke(entity, @params);
            }
            finally
            {
                if (@params != null)
                {
                    _paramArrayPool.Return(@params);
                }
            }
        }

        #region Helpers

        private MethodInfo GetOrCacheEntitySetMethod(Type componentType)
        {
            if (!_entitySetByTypeCache.TryGetValue(componentType, out var entitySetMethod))
            {
                entitySetMethod = EntitySetMethodInfo.MakeGenericMethod(componentType);
                _entitySetByTypeCache.Add(componentType, entitySetMethod);
            }

            return entitySetMethod;
        }

        private static void ThrowFailedToFindComponentType(string componentName) =>
            throw new InvalidOperationException($"Failed to find type for component name = {componentName}, this is not supposed to happen and is likely a bug.");

        #endregion
    }
}
