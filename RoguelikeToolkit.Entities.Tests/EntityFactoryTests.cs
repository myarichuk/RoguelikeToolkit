using System.Linq;
using DefaultEcs;
using RoguelikeToolkit.Entities.Components;
using Xunit;

// ReSharper disable ExceptionNotDocumented

namespace RoguelikeToolkit.Entities.Tests
{
	[Component]
	public class Foobar
	{
		public int NumProperty { get; set; }
		public string StringProperty { get; set; }
	}

	[Component]
	public class Barfoo
	{
		public int AnotherNumProperty { get; set; }
		public string AnotherStringProperty { get; set; }
	}

	[Component(IsGlobal = true)]
	public class Attributes
	{
		public int Strength { get; set; }
		public int Agility { get; set; }
	}

	public class Foo : IValueComponent<string>
	{
		public string Value { get; set; }
	}

	public struct AnotherFoobar : IValueComponent<int>
	{
		public int Value { get; set; }
	}

	[Component(Name = "Health")]
	public struct UnitHealth : IValueComponent<decimal>
	{
		public decimal Value { get; set; }
	}

	public class EntityFactoryTests
	{
		private readonly EntityFactory _entityFactory;
		private readonly EntityTemplateRepository _templateRepository = new();

		public EntityFactoryTests()
		{
			_templateRepository.LoadTemplateFolder("TemplatesForLoading", ignoreLoadingErrors:true);
			_entityFactory = new EntityFactory(_templateRepository, new World());
		}

		[Fact]
		public void Should_return_false_non_existing_template_name() =>
			Assert.False(_entityFactory.TryCreate("non-existing", out _));

		[Fact]
		public void HasTemplateFor_should_properly_work()
		{
			Assert.False(_entityFactory.HasTemplateFor("non-existing"));
			Assert.True(_entityFactory.HasTemplateFor("template-simple"));
		}

		[Fact]
		public void Can_create_simple_entity()
		{
			Assert.True(_entityFactory.TryCreate("template-simple", out var entity));

			Assert.True(entity.Has<Foobar>());
			var fetchedFoobar = entity.Get<Foobar>();
			Assert.Equal(123, fetchedFoobar.NumProperty);
			Assert.Equal("abcdef", fetchedFoobar.StringProperty);
		}

		[Fact]
		public void Can_create_entity_with_global_component()
		{
			Assert.True(_entityFactory.TryCreate("template-with-global", out var entityA));
			Assert.True(_entityFactory.TryCreate("template-with-global", out var entityB));

			var attributesA = entityA.Get<Attributes>();
			var attributesB = entityB.Get<Attributes>();

			Assert.Same(attributesA, attributesB);
		}

		[Fact]
		public void Can_create_simple_entity_custom_name()
		{
			Assert.True(_entityFactory.TryCreate("template-simple3", out var entity));

			Assert.True(entity.Has<UnitHealth>());
			var health = entity.Get<UnitHealth>();
			Assert.Equal((decimal)123.3, health.Value);
		}

		[Fact]
		public void Can_create_complex_entity()
		{
			Assert.True(_entityFactory.TryCreate("template-with-embedded", out var entity));

			//sanity check
			Assert.True(entity.Has<Foobar>());
			
			var entityTemplate2 = entity.GetChildren().FirstOrDefault(e => e.Has<Barfoo>() && e.Has<Foo>());
			Assert.NotEqual(default, entityTemplate2);

			var barfooComponent = entityTemplate2.Get<Barfoo>();
			Assert.Equal("defgh", barfooComponent.AnotherStringProperty);
			Assert.Equal(234, barfooComponent.AnotherNumProperty);

			var valueComponent = entityTemplate2.Get<Foo>();
			Assert.Equal("this is a test!", valueComponent.Value);
		}

		[Fact]
		public void Can_create_entity_with_inherit()
		{
			Assert.True(_entityFactory.TryCreate("template-with-inherit", out var entity));

			Assert.True(entity.Has<Foobar>());
			var fetchedFoobar = entity.Get<Foobar>();
			Assert.Equal(123, fetchedFoobar.NumProperty);
			Assert.Equal("abcdef", fetchedFoobar.StringProperty);

			var barfooComponent = entity.Get<Barfoo>();
			Assert.Equal("defgh", barfooComponent.AnotherStringProperty);
			Assert.Equal(234, barfooComponent.AnotherNumProperty);

			var valueComponent = entity.Get<Foo>();
			Assert.Equal("this is a test!", valueComponent.Value);
		}

		[Fact]
		public void Can_create_entity_with_two_level_inherit()
		{
			Assert.True(_entityFactory.TryCreate("template-with-inherit-two-levels", out var entity));

			Assert.True(entity.Has<Foobar>());
			var fetchedFoobar = entity.Get<Foobar>();
			Assert.Equal(123, fetchedFoobar.NumProperty);
			Assert.Equal("abcdef", fetchedFoobar.StringProperty);

			var barfooComponent = entity.Get<Barfoo>();
			Assert.Equal("defgh", barfooComponent.AnotherStringProperty);
			Assert.Equal(234, barfooComponent.AnotherNumProperty);

			var valueComponent = entity.Get<Foo>();
			Assert.Equal("this is a test!", valueComponent.Value);

			var valueComponent2 = entity.Get<AnotherFoobar>();
			Assert.Equal(123, valueComponent2.Value);
		}
	}
}
