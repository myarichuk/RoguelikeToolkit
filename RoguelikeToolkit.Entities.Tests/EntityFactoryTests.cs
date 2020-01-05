using RoguelikeToolkit.Entities.BuiltinComponents;
using Xunit;

namespace RoguelikeToolkit.Entities.Tests
{
    public class EntityFactoryTests
    {
        private readonly EntityTemplateCollection templateCollection;
        private readonly EntityFactory entityFactory;
        public EntityFactoryTests()
        {
            templateCollection = new EntityTemplateCollection(".");
            entityFactory = new EntityFactory(new DefaultEcs.World(), templateCollection);
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
    }
}
