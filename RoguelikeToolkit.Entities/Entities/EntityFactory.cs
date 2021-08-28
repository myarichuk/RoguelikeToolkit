using DefaultEcs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RoguelikeToolkit.Entities.Entities
{
    public class EntityFactory
    {
        private readonly World _world;
        private readonly EntityTemplateCollection _templateCollection;
        private readonly InheritanceGraph _inheritanceGraph;
        private readonly EntityComponentAttacher _componentAttacher;
        private readonly Dictionary<string, EffectiveEntityTemplate> _effectiveEntityTemplateCache = new(StringComparer.InvariantCultureIgnoreCase);


        public EntityFactory(
            World world,
            EntityTemplateCollection templateCollection,
            params Assembly[] componentAssemblies) : this(world, templateCollection, DefaultComponentNameExtractor, null, componentAssemblies)
        {
        }

        public EntityFactory(
            World world, 
            EntityTemplateCollection templateCollection, 
            Func<Type, string> componentNameExtractor,
            ComponentFactoryOptions? componentFactoryOptions,
            params Assembly[] componentAssemblies)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _templateCollection = templateCollection ?? throw new ArgumentNullException(nameof(templateCollection));
            _componentAttacher = new EntityComponentAttacher(componentNameExtractor, componentFactoryOptions, componentAssemblies);

            if (_templateCollection.Templates.Any() == false)
                throw new InvalidOperationException("Template collection is empty, I don't think it is supposed to happen, this is likely a bug.");

            _inheritanceGraph = new InheritanceGraph(_templateCollection.Templates);


        }

        public bool TryCreateEntity(string entityId, out Entity entity)
        {
            entity = default;
            if(_templateCollection.TryGetById(entityId, out var template))
            {
                entity = _world.CreateEntity();
                
                if(!_effectiveEntityTemplateCache.TryGetValue(entityId, out var effectiveTemplate))
                {
                    effectiveTemplate = new EffectiveEntityTemplate(template, _inheritanceGraph.GetInheritanceChainFor(template));
                    _effectiveEntityTemplateCache.Add(entityId, effectiveTemplate);
                }

                var components = effectiveTemplate.Components;
                foreach(var componentKV in components)
                    _componentAttacher.InstantiateAndAttachComponent(componentKV.Key, componentKV.Value, ref entity);


                return true;
            }

            return false;
        }


        #region Helpers

        private static string DefaultComponentNameExtractor(Type componentType)
        {
            var componentAttribute = componentType.GetCustomAttribute<ComponentAttribute>();

            return componentAttribute?.Name ?? DefaultGetComponentName(componentType.Name);
            string DefaultGetComponentName(string name) => name.EndsWith("Component") ? name.Replace("Component", string.Empty) : name;
        }

        #endregion
    }
}
