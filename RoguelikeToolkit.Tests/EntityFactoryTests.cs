using System;
using DefaultEcs;
using RoguelikeToolkit.Common.Entities;
using RoguelikeToolkit.Common.EntityTemplates;
using RoguelikeToolkit.Tryouts;
using Xunit;

namespace RoguelikeToolkit.Tests
{
    public class EntityFactoryTests
    {
        [Fact]
        public void Can_resolve_from_multi_level_hierarchical_template()
        {
            var entityTemplateRepository = new EntityTemplateRepository("Templates");
            var entityFactory = new EntityFactory(entityTemplateRepository);
            var world = new World();

            var actorEntity = world.CreateEntity();

            var created = entityFactory.TryCreate("actor", world, ref actorEntity);
            Assert.True(created);

            ref var raceComponent =  ref actorEntity.Get<RaceComponent>();
            Assert.Equal("Human",raceComponent.Value);

            ref var attributesComponent = ref actorEntity.Get<AttributesComponent>();
            Assert.Equal(10, attributesComponent.Strength);
            Assert.Equal("10", attributesComponent.Agility);
            Assert.Equal(10.5, attributesComponent.Intelligence);
            Assert.Equal(12.34, attributesComponent.Endurance);
            Assert.Equal(0, attributesComponent.NonExistingProperty);
        }
    }
}
