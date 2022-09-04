using DefaultEcs;
using System;

namespace RoguelikeToolkit.Entities.Factory
{
	public class EntityFactory
	{
		private readonly EntityTemplateRepository _entityRepository;
		private readonly World _world;

		public EntityFactory(EntityTemplateRepository entityRepository, World world)
		{
			_entityRepository = entityRepository ?? throw new ArgumentNullException(nameof(entityRepository));
			_world = world ?? throw new ArgumentNullException(nameof(world));
		}

		public bool TryCreate(string entityName, out Entity entity)
		{
			entity = default;
			if (_entityRepository.TryGetByName(entityName, out var template))
			{

				return true;
			}

			return false;
		}
	}
}
