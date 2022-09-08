using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

	public class Foo: IValueComponent<string>
	{
		public string Value { get; set; }
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
	}
}
