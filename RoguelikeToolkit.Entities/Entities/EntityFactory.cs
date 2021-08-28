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
                if (!TryCreatEntityFrom(template, out entity))
                    throw new InvalidOperationException($"Failed to create an entity from template Id = {template.Id}");

                return true;
            }

            return false;
        }

        private bool TryCreatEntityFrom(EntityTemplate template, out Entity entity, EntityTemplate parent = null)
        {
            entity = _world.CreateEntity();
            if (!_effectiveEntityTemplateCache.TryGetValue(template.Id, out var effectiveTemplate))
            {
                effectiveTemplate = new EffectiveEntityTemplate(template, _inheritanceGraph.GetInheritanceChainFor(template));
                _effectiveEntityTemplateCache.Add(template.Id, effectiveTemplate);
            }

            foreach (var componentKV in effectiveTemplate.Components)
                _componentAttacher.InstantiateAndAttachComponent(componentKV.Key, componentKV.Value, ref entity);

            foreach (var childEntityKV in effectiveTemplate.ChildEntities)
            {
                if (!TryCreatEntityFrom(childEntityKV.Value, out var childEntity, template))
                    throw new InvalidOperationException($"Failed to create an entity from template Id = {template.Id}");

                entity.SetAsParentOf(childEntity);
            }

            return true;
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
