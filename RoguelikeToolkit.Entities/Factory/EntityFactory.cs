using DefaultEcs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using deniszykov.TypeConversion;
using Fasterflect;
using Microsoft.Extensions.Options;
using RoguelikeToolkit.Entities.Factory;

namespace RoguelikeToolkit.Entities
{
	public class EntityFactory
	{
		private readonly EntityTemplateRepository _entityRepository;
		private readonly ComponentFactory _componentFactory = new();
		private readonly ComponentTypeRegistry _componentTypeRegistry = new();
		private readonly EntityInheritanceResolver _inheritanceResolver;
		private readonly World _world;

		private static readonly ConcurrentDictionary<Type, MethodInfo> EntitySetMethodCache = new();
		private static readonly MethodInfo EntitySetMethod =
			typeof(Entity).Methods(nameof(Entity.Set))
				.FirstOrDefault(m => m.Parameters().Count == 1);

		private readonly TypeConversionProvider _typeConversionProvider = new(Options.Create(new TypeConversionProviderOptions
		{
			Options = ConversionOptions.UseDefaultFormatIfNotSpecified
		}));

		/// <exception cref="ArgumentNullException">entityRepository or world parameter is null.</exception>
		/// <exception cref="InvalidOperationException">Failed to detect Entity::Set(ref T param) method, this probably means DefaultEcs was updated and had a breaking change. This is not supposed to happen.</exception>
		public EntityFactory(EntityTemplateRepository entityRepository, World world)
		{
			_entityRepository = entityRepository ?? throw new ArgumentNullException(nameof(entityRepository));
			_world = world ?? throw new ArgumentNullException(nameof(world));
			_inheritanceResolver = new EntityInheritanceResolver(_entityRepository.TryGetByName);

			//sanity check
			if (EntitySetMethod == null)
			{
				throw new InvalidOperationException("Failed to detect Entity::Set<T>(ref T param) method, this probably means DefaultEcs was updated and had a breaking change. This is not supposed to happen and should be reported");
			}
		}


		/// <exception cref="ArgumentNullException">templateName is <see langword="null"/></exception>
		public bool TryCreate(string entityName, out Entity entity)
		{
			entity = default;
			
			return _entityRepository.TryGetByName(entityName, out var rootTemplate) &&
			       TryCreate(rootTemplate, out entity);
		}

		/// <exception cref="ArgumentNullException"><paramref name="rootTemplate"/> is <see langword="null"/></exception>
		public bool TryCreate(EntityTemplate rootTemplate, out Entity entity)
		{
			if (rootTemplate == null)
				throw new ArgumentNullException(nameof(rootTemplate));

			var graphVisitor = new EntityGraphVisitor(rootTemplate);

			ConstructEntity(rootTemplate, out var rootEntity);

			graphVisitor.Traverse(template =>
			{
				if (template.Name == rootTemplate.Name)
					return;

				if (TryCreate(template, out var childEntity))
					rootEntity.SetAsParentOf(childEntity);
			});

			entity = rootEntity;

			return true;
		}

		//TODO: refactor to reduce cognitive complexity
		private void ConstructEntity(EntityTemplate template, out Entity entity)
		{
			entity = _world.CreateEntity();

			foreach (var componentRawData in template.Components)
			{
				if (!_componentTypeRegistry.TryGetComponentType(componentRawData.Key, out var componentType))
					throw new InvalidOperationException(
						$"Component type '{componentRawData.Key}' is not registered. Check the spelling of the component name in the template.");

				var rawComponentType = componentRawData.Value.GetType();
				if (rawComponentType.IsValueType || rawComponentType.Name == nameof(String))
				{

				}
				else
				{

					if (componentRawData.Value is not Dictionary<object, object> componentObjectData)
						throw new InvalidOperationException(
							"Invalid data received from the template after deserialization. This is not supposed to happen and is likely a bug.");

					//TODO: refactor for better error handling
					if (!_componentFactory.TryCreateInstance(componentType, componentObjectData,
						    out var componentInstance))
					{
						throw new InvalidOperationException(
							$"Failed to create an instance of a component (type = {componentType.FullName})");
					}

					//TODO: ensure this line works properly, probably it doesn't
					var genericEntitySetMethod =
						EntitySetMethodCache.GetOrAdd(componentType, type => EntitySetMethod.MakeGenericMethod(type));

					genericEntitySetMethod.Call(entity.WrapIfValueType(),
						_typeConversionProvider.Convert(typeof(object), componentType, componentInstance));
				}
			}
		}
	}
}
