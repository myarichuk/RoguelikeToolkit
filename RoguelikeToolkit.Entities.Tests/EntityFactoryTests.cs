using RoguelikeToolkit.Entities.BuiltinComponents;
using System.Linq;
using Xunit;

namespace RoguelikeToolkit.Entities.Tests
{
    public class EntityFactoryTests
    {
        private readonly EntityTemplateCollection templateCollection;
        private readonly EntityFactory entityFactory;
        public EntityFactoryTests()
        {
            templateCollection = new EntityTemplateCollection("Templates");
            entityFactory = new EntityFactory(new DefaultEcs.World(), templateCollection);
        }

        [Fact]
        public void Can_parse_all_templates()
        {
            Assert.Empty(templateCollection.TemplatesFailedToParse);
        }

        [Fact]
        public void Can_build_from_embedded_template()
        {
            Assert.True(entityFactory.TryCreateEntity("object", out var entity));
            Assert.True(entity.Has<WeightComponent>());
            Assert.True(entity.Has<HealthComponent>());

            Assert.Equal(1.0, entity.Get<WeightComponent>().Value);
            Assert.Equal(100.0, entity.Get<HealthComponent>().Value);
        }

        [Fact]
        public void Can_build_complex_entity()
        {
            Assert.True(entityFactory.TryCreateEntity("actor", out var actorEntity));

            Assert.True(actorEntity.Has<AttributesComponent>());
            Assert.Equal(5, actorEntity.Get<AttributesComponent>().Strength);
            Assert.Equal(7, actorEntity.Get<AttributesComponent>().Agility);
            var childEntities = actorEntity.GetChildren().ToArray();
            Assert.Equal(2, childEntities.Length);

            foreach (var childEntity in actorEntity.GetChildren())
            {
                Assert.True(childEntity.Has<WeightComponent>());
                Assert.True(childEntity.Has<HealthComponent>());
                Assert.True(childEntity.Has<DirtComponent>());

                Assert.Equal(0.0, childEntity.Get<DirtComponent>().Value);
                Assert.Equal(10.0, childEntity.Get<WeightComponent>().Value);
                Assert.Equal(100.0, childEntity.Get<HealthComponent>().Value);
            }
        }
    }
}
