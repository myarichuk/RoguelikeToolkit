using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using DefaultEcs;
using Xunit;

namespace RoguelikeToolkit.Entities.Tests
{
    public class WeightComponent : IValueComponent<double>
    {
        public double Value { get; set; }
    }

    public class HealthComponent : IValueComponent<double>
    {
        public double Value { get; set; }
    }

    public class DirtComponent : IValueComponent<double>
    {
        public double Value { get; set; }
    }

    public class EntityFactoryTests
    {
        private readonly EntityFactory _entityFactory;
        public EntityFactoryTests()
        {
            _entityFactory = EntityFactory.Construct()
                .WithTemplatesFrom("Templates")
                .Build();
        }

        [Fact]
        public void Can_parse_all_templates() => Assert.NotEmpty(new EntityTemplateCollection("Templates").Templates);

        [Fact]
        public void Can_build_from_embedded_template()
        {
            Assert.True(_entityFactory.TryCreateEntity("object", out var entity));
            Assert.True(entity.Has<WeightComponent>());
            Assert.True(entity.Has<HealthComponent>());

            Assert.Equal(1.0, entity.Get<WeightComponent>().Value);
            Assert.Equal(100.0, entity.Get<HealthComponent>().Value);
        }

        //[Fact]
        //public async Task Can_build_with_scipt()
        //{
        //    Assert.True(_entityFactory.TryCreateEntity("ActorWithScripts", out var entity));

        //    await entity.RunScriptAsync<ActionScriptComponent>(c => c.Value);

        //    Assert.NotEqual(0, entity.Get<ActionChanceComponent>().Result);
        //}

        //[Fact]
        //public async Task Can_build_with_component_scipt()
        //{
        //    Assert.True(_entityFactory.TryCreateEntity("ActorWithScripts", out var entity));

        //    await entity.RunScriptAsync<ActionChanceComponent>(c => c.ActionScript);

        //    Assert.NotEqual(0, entity.Get<ActionChanceComponent>().Result);
        //}

        //[Fact]
        //public async Task Can_build_with_interaction_scipt()
        //{
        //    Assert.True(_entityFactory.TryCreateEntity("actorWithAbility", out var player));
        //    Assert.True(_entityFactory.TryCreateEntity("object2", out var enemy));

        //    Assert.Equal(100.0, enemy.Get<HealthComponent>().Value);
        //    await player.RunScriptAsync<KickAbility>(enemy, c => c.Effect);
        //    Assert.NotEqual(100.0, enemy.Get<HealthComponent>().Value);

        //}

        //[Fact]
        //public async Task Can_build_with_interaction_scipt_from_file()
        //{
        //    Assert.True(_entityFactory.TryCreateEntity("actorWithAbility2", out var player));
        //    Assert.True(_entityFactory.TryCreateEntity("object2", out var enemy));

        //    Assert.Equal(100.0, enemy.Get<HealthComponent>().Value);
        //    await player.RunScriptAsync<KickAbility>(enemy, c => c.Effect);
        //    Assert.NotEqual(100.0, enemy.Get<HealthComponent>().Value);

        //}

        [Fact]
        public void Can_build_from_template_with_converting_strings_to_numbers()
        {
            Assert.True(_entityFactory.TryCreateEntity("object2", out var entity));
            Assert.True(entity.Has<WeightComponent>());
            Assert.True(entity.Has<HealthComponent>());
            Assert.True(entity.Has<FooBarComponent>());

            Assert.Equal(1.0, entity.Get<WeightComponent>().Value);
            Assert.Equal(100.0, entity.Get<HealthComponent>().Value);
            Assert.Equal(555, entity.Get<FooBarComponent>().Value);
        }

        [Fact]
        public void Can_build_value_component_with_dictionary_as_value()
        {
            Assert.True(_entityFactory.TryCreateEntity("actor5", out var actorEntity));
            var valueComponent = actorEntity.Get<AttributesAsValueTypeComponent>();
            Assert.NotNull(valueComponent.Value);
            Assert.Equal(1, valueComponent.Value["Foo"]);
            Assert.Equal(2, valueComponent.Value["Bar"]);
            Assert.Equal(123, valueComponent.Value["Baz"]);
        }

        [Fact]
        public void Can_build_value_component_with_dictionary_with_enum_as_value()
        {
            Assert.True(_entityFactory.TryCreateEntity("actor9", out var actorEntity));
            var valueComponent = actorEntity.Get<AttributesWithEnumAsValueTypeComponent>();
            Assert.NotNull(valueComponent.Value);

            Assert.Equal(KnownColor.Indigo, valueComponent.Value["Foo"]);
            Assert.Equal(KnownColor.Azure, valueComponent.Value["Bar"]);
        }

        [Fact]
        public void Can_build_with_component_array_type_property()
        {
            Assert.True(_entityFactory.TryCreateEntity("actor11", out var actorEntity));

            var valueComponent = actorEntity.Get<TestMetadataComponent>();

            Assert.Equal("foo", valueComponent.Tags[0]);
            Assert.Equal("bar", valueComponent.Tags[1]);
            Assert.Equal("hello", valueComponent.Tags[2]);
        }

        [Fact]
        public void Can_build_with_calculated_dice_field()
        {
            Assert.True(_entityFactory.TryCreateEntity("actor12", out var actorEntity));

            var leftArm = actorEntity.GetChildren().First(e => e.Id().Contains("LeftArm"));

            Assert.True(leftArm.TryGet<WeightComponent>(out var weight));
            Assert.True(weight.Value > 12);
        }


        [Fact]
        public void Can_build_value_component_with_list_as_value()
        {
            Assert.True(_entityFactory.TryCreateEntity("actor6", out var actorEntity));

            var valueComponent = actorEntity.Get<AttributeAsListComponent>();

            Assert.Equal(3, valueComponent.Value[0]);
            Assert.Equal(2, valueComponent.Value[1]);
            Assert.Equal(1, valueComponent.Value[2]);
        }

        [Fact]
        public void Should_throw_when_value_component_with_interface_as_value() =>
            Assert.Throws<InvalidOperationException>(() => _entityFactory.TryCreateEntity("actor10", out var actorEntity));

        [Fact]
        public void Can_build_value_component_with_hashset_of_enums_as_value()
        {
            Assert.True(_entityFactory.TryCreateEntity("actor8", out var actorEntity));

            var valueComponent = actorEntity.Get<AttributeAsHashSetComponent>();

            Assert.Equal(2, valueComponent.Value.Count);
            Assert.Equal(KnownColor.Azure, valueComponent.Value.First());
            Assert.Equal(KnownColor.Indigo, valueComponent.Value.Last());
        }

        [Fact]
        public void Can_build_value_component_with_enum_as_value()
        {
            Assert.True(_entityFactory.TryCreateEntity("actor7", out var actorEntity));

            var valueComponent = actorEntity.Get<AttributeAsEnumComponent>();

            Assert.Equal(KnownColor.Azure, valueComponent.Value);
        }

        [Fact]
        public void Can_build_complex_entity()
        {
            Assert.True(_entityFactory.TryCreateEntity("actor", out var actorEntity));

            Assert.Single(actorEntity.Get<MetadataComponent>().Value);
            Assert.Equal("actor", actorEntity.Get<MetadataComponent>().Value.First());

            foreach (var childEntity in actorEntity.GetChildren())
            {
                var metadata = childEntity.Get<MetadataComponent>().Value;
                Assert.Equal(2, metadata.Count);
                Assert.Contains("actor", metadata);
                Assert.Contains("arm", metadata);
            }

            ValidateActorEntity(actorEntity);
        }

        private static void ValidateActorEntity(Entity actorEntity)
        {
            Assert.True(actorEntity.Has<AttributesComponent>());
            Assert.True(actorEntity.Has<MetadataComponent>());

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
            Assert.True(_entityFactory.TryCreateEntity("actor2", out var actorEntity));
            ValidateActorEntity(actorEntity);
        }


        [Fact]
        public void Can_convert_int_to_double_when_loading()
        {
            Assert.True(_entityFactory.TryCreateEntity("actor3", out var actorEntity));
            Assert.True(actorEntity.Has<Attributes2Component>());
            Assert.Equal(5, actorEntity.Get<Attributes2Component>().Strength);
            Assert.Equal(7, actorEntity.Get<Attributes2Component>().Agility);
            Assert.Equal(10.0, actorEntity.Get<Attributes2Component>().Wisdom);
        }

        [Fact]
        public void Can_convert_double_to_int_when_loading()
        {
            Assert.True(_entityFactory.TryCreateEntity("actor4", out var actorEntity));
            Assert.True(actorEntity.Has<Attributes3Component>());
            Assert.Equal(5, actorEntity.Get<Attributes3Component>().Strength);
            Assert.Equal(7, actorEntity.Get<Attributes3Component>().Agility);
            Assert.Equal(10, actorEntity.Get<Attributes3Component>().Wisdom);
        }
    }
}
