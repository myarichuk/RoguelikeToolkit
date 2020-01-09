using DefaultEcs;
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
        public void Can_build_value_component_with_dictionary_as_value()
        {
            Assert.True(entityFactory.TryCreateEntity("actor5", out var actorEntity));
            var valueComponent = actorEntity.Get<AttributesAsValueTypeComponent>();
            Assert.Equal(1,valueComponent.Value["Foo"]);
            Assert.Equal(2,valueComponent.Value["Bar"]);
            Assert.Equal(123,valueComponent.Value["Baz"]);
        }

        [Fact]
        public void Can_build_complex_entity()
        {
            Assert.True(entityFactory.TryCreateEntity("actor", out var actorEntity));
            ValidateActorEntity(actorEntity);
        }

        private static void ValidateActorEntity(Entity actorEntity)
        {
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

        [Fact]
        public void Can_ignore_non_existing_component_fields()
        {
            Assert.True(entityFactory.TryCreateEntity("actor2", out var actorEntity));
            ValidateActorEntity(actorEntity);
        }


        [Fact]
        public void Can_convert_int_to_double_when_loading()
        {
            Assert.True(entityFactory.TryCreateEntity("actor3", out var actorEntity));
            Assert.True(actorEntity.Has<Attributes2Component>());
            Assert.Equal(5, actorEntity.Get<Attributes2Component>().Strength);
            Assert.Equal(7, actorEntity.Get<Attributes2Component>().Agility);
            Assert.Equal(10.0, actorEntity.Get<Attributes2Component>().Wisdom);
        }

        [Fact]
        public void Can_convert_double_to_int_when_loading()
        {
            Assert.True(entityFactory.TryCreateEntity("actor4", out var actorEntity));
            Assert.True(actorEntity.Has<Attributes3Component>());
            Assert.Equal(5, actorEntity.Get<Attributes3Component>().Strength);
            Assert.Equal(7, actorEntity.Get<Attributes3Component>().Agility);
            Assert.Equal(10, actorEntity.Get<Attributes3Component>().Wisdom);
        }
    }
}
