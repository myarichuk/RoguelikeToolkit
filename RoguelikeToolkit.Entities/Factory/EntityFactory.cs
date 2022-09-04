using DefaultEcs;
using System;

namespace RoguelikeToolkit.Entities.Factory
{
	public class EntityFactory
	{
		private readonly EntityTemplateRepository _entityRepository;
		private readonly ComponentFactory _componentFactory = new();
		private readonly EntityInheritanceResolver _inheritanceResolver;
		private readonly World _world;

		/// <exception cref="ArgumentNullException">entityRepository or world parameter is null.</exception>
		public EntityFactory(EntityTemplateRepository entityRepository, World world)
		{
			_entityRepository = entityRepository ?? throw new ArgumentNullException(nameof(entityRepository));
			_world = world ?? throw new ArgumentNullException(nameof(world));
			_inheritanceResolver = new EntityInheritanceResolver(_entityRepository.TryGetByName);
		}

		/// <exception cref="InvalidOperationException">Max number of <see cref="Entity" /> reached.</exception>
		public bool TryCreate(string entityName, out Entity entity)
		{
			entity = default;
			if (!_entityRepository.TryGetByName(entityName, out var template))
				return false;

			entity = _world.CreateEntity();


			return true;

		}
	}
}
