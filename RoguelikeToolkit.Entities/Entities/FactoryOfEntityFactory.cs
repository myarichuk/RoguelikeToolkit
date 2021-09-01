using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Antlr4.Runtime.Atn;
using DefaultEcs;

namespace RoguelikeToolkit.Entities.Entities
{
    public class FactoryOfEntityFactory
    {
        private Assembly[] _extraComponentAssemblies;
        private readonly List<Assembly> _extraPropertyMapperAssemblies = new();

        private readonly List<Type> _extraComponentTypes = new();
        private readonly List<Type> _extraPropertyMapperTypes = new();
        private readonly MapperRepository _mapperRepository;

        private World _world;
        private EntityTemplateCollection _templateCollection;
        private EntityFactoryOptions _options = EntityFactoryOptions.Default;

        internal FactoryOfEntityFactory()
        {
            _mapperRepository = new MapperRepository(new ThisAssemblyResolver());
        }

        public FactoryOfEntityFactory WithTypeMapperResolvers(params IMapperResolver[] mapperResolvers)
        {
            ThrowIfAnyMemberNull(mapperResolvers ?? throw new ArgumentNullException(nameof(mapperResolvers)));

            foreach (var resolver in mapperResolvers)
                _mapperRepository.RegisterFrom(resolver);
            
            return this;
        }

        public FactoryOfEntityFactory WithTemplatesFrom(params string[] templateFolders)
        {
            ThrowIfAnyMemberNull(templateFolders ?? throw new ArgumentNullException(nameof(templateFolders)));
            _templateCollection = new EntityTemplateCollection(templateFolders);
            return this;
        }

        public FactoryOfEntityFactory WithWorld(World world)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
            return this;
        }

        public FactoryOfEntityFactory WithPropertyMapperTypes(params Type[] mapperTypes)
        {
            ThrowIfAnyMemberNull(mapperTypes ?? throw new ArgumentNullException(nameof(mapperTypes)));
            _extraPropertyMapperTypes.AddRange(mapperTypes);

            return this;
        }

        public FactoryOfEntityFactory WithComponentTypes(params Type[] componentTypes)
        {
            ThrowIfAnyMemberNull(componentTypes ?? throw new ArgumentNullException(nameof(componentTypes)));
            _extraComponentTypes.AddRange(componentTypes);

            return this;
        }

        public FactoryOfEntityFactory WithPropertyMappersFromAssemblies(params Assembly[] assemblies)
        {
            ThrowIfAnyMemberNull(assemblies ?? throw new ArgumentNullException(nameof(assemblies)));
            _extraPropertyMapperAssemblies.AddRange(assemblies);

            return this;
        }

        public FactoryOfEntityFactory WithComponentFromAssemblies(params Assembly[] assemblies)
        {
            ThrowIfAnyMemberNull(assemblies ?? throw new ArgumentNullException(nameof(assemblies)));
            _extraComponentAssemblies = assemblies;

            return this;
        }

        public EntityFactory Build() =>
#pragma warning disable CS0618 // Type or member is obsolete
            new EntityFactory(
                _world ?? new World(),
                _templateCollection ?? throw new InvalidOperationException($"Cannot build {nameof(EntityFactory)} without specifying from where to load entity templates"),
                _options,
                _mapperRepository ?? new MapperRepository(new ThisAssemblyResolver()),
                _extraComponentAssemblies ?? Array.Empty<Assembly>());
#pragma warning restore CS0618 // Type or member is obsolete

        private static void ThrowIfAnyMemberNull<TItem>(IEnumerable<TItem> collection)
        {
            if (collection.Any(asm => asm == null))
                throw new ArgumentException($"At least item in {nameof(collection)} is null, this is not supported.");
        }
    }
}
