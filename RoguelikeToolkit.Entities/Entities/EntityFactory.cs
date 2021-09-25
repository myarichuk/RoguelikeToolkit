using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DefaultEcs;
using RoguelikeToolkit.Entities.Components;
using RoguelikeToolkit.Entities.Entities;

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
        private readonly Dictionary<EffectiveEntityTemplate, IdComponent> _idCache = new();

        [Obsolete("EntityFactor::ctor() is obsolete, please use EntityFactory::Construct() instead. Next version, this constructor will be turned into 'internal'")]
        public EntityFactory(
            World world,
            EntityTemplateCollection templateCollection,
            EntityFactoryOptions options = null,
            MapperRepository mapperRepository = null,
            params Assembly[] componentAssemblies) : this(world, templateCollection, DefaultComponentNameExtractor, options ?? EntityFactoryOptions.Default, mapperRepository, componentAssemblies)
        {
        }

        public static FactoryOfEntityFactory Construct() => new FactoryOfEntityFactory();


        [Obsolete("EntityFactor::ctor() is obsolete, please use EntityFactory::Construct() instead. Next version, this constructor will be turned into 'internal'")]
        public EntityFactory(
            World world,
            EntityTemplateCollection templateCollection,
            Func<Type, string> componentNameExtractor,
            EntityFactoryOptions options = null,
            MapperRepository mapperRepository = null,
            params Assembly[] componentAssemblies)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _templateCollection = templateCollection ?? throw new ArgumentNullException(nameof(templateCollection));
            
            _options = options ?? EntityFactoryOptions.Default;
            mapperRepository = mapperRepository ?? new MapperRepository(new ThisAssemblyResolver());
            _componentAttacher = new EntityComponentAttacher(componentNameExtractor, mapperRepository, componentAssemblies);

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
                .GetOrAdd(template, templ => 
                    new EffectiveEntityTemplate(templ, _inheritanceGraph.GetInheritanceChainFor(templ)));

            foreach (var componentKV in effectiveTemplate.Components)
                _componentAttacher.InstantiateAndAttachComponent(
                    componentKV.Key, componentKV.Value, _options, ref entity);

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

            if (_options.AutoIncludeIdComponent && entity.Has<IdComponent>() == false)
            {
                var id = _idCache.GetOrAdd(effectiveTemplate, effTempl => new IdComponent { Value = effTempl.Id });
                entity.Set(id);
            }

            return true;
        }

        #region Helpers

        private static string DefaultComponentNameExtractor(Type componentType)
        {
            var componentAttribute = componentType.GetCustomAttribute<ComponentAttribute>();

            return componentAttribute?.Name ?? DefaultGetComponentName(componentType.Name);

            static string DefaultGetComponentName(string name) => 
                name.EndsWith("Component") ? name.Replace("Component", string.Empty) : name;
        }

        #endregion
    }
}
