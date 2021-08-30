using DefaultEcs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RoguelikeToolkit.Entities
{
    public class EntityFactory
    {
        private readonly World _world;
        private readonly EntityTemplateCollection _templateCollection;
        private readonly EntityFactoryOptions _options;
        private readonly InheritanceGraph _inheritanceGraph;
        private readonly EntityComponentAttacher _componentAttacher;
        private readonly Dictionary<EntityTemplate, EffectiveEntityTemplate> _effectiveEntityTemplateCache = new();
        private readonly Dictionary<EffectiveEntityTemplate, MetadataComponent> _metadataCache = new();

        public EntityFactory(
            World world,
            EntityTemplateCollection templateCollection,
            EntityFactoryOptions options = null,
            params Assembly[] componentAssemblies) : this(world, templateCollection, DefaultComponentNameExtractor, null, options ?? EntityFactoryOptions.Default, componentAssemblies)
        {
        }

        public EntityFactory(
            World world,
            EntityTemplateCollection templateCollection,
            Func<Type, string> componentNameExtractor,
            ComponentFactoryOptions? componentFactoryOptions,
            EntityFactoryOptions options = null,
            params Assembly[] componentAssemblies)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _templateCollection = templateCollection ?? throw new ArgumentNullException(nameof(templateCollection));
            
            _options = options ?? EntityFactoryOptions.Default;

            _componentAttacher = new EntityComponentAttacher(componentNameExtractor, componentFactoryOptions, componentAssemblies);

            if (_templateCollection.Templates.Any() == false)
            {
                throw new InvalidOperationException("Template collection is empty, I don't think it is supposed to happen, this is likely a bug.");
            }

            _inheritanceGraph = new InheritanceGraph(_templateCollection.Templates);
        }

        public bool TryCreateEntity(string entityId, out Entity entity)
        {
            entity = default;
            if (_templateCollection.TryGetById(entityId, out var template))
            {
                if (!TryCreatEntityFrom(template, out entity))
                {
                    throw new InvalidOperationException($"Failed to create an entity from template Id = {template.Id}");
                }

                return true;
            }

            return false;
        }

        private bool TryCreatEntityFrom(EntityTemplate template, out Entity entity)
        {
            entity = _world.CreateEntity();
            var effectiveTemplate = _effectiveEntityTemplateCache
                .GetOrAdd(template, t => new EffectiveEntityTemplate(t, _inheritanceGraph.GetInheritanceChainFor(t)));

            foreach (var componentKV in effectiveTemplate.Components)
            {
                _componentAttacher.InstantiateAndAttachComponent(componentKV.Key, componentKV.Value, _options, ref entity);
            }

            foreach (var childEntityKV in effectiveTemplate.ChildEntities)
            {
                if (TryCreatEntityFrom(childEntityKV.Value, out var childEntity) == false)
                {
                    throw new InvalidOperationException($"Failed to create an entity from template Id = {template.Id}");
                }

                entity.SetAsParentOf(childEntity);
            }

            if (entity.Has<MetadataComponent>() == false)
            {
                var metadata = _metadataCache.GetOrAdd(effectiveTemplate, t => new MetadataComponent { Value = t.Tags });
                entity.Set(metadata);
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
