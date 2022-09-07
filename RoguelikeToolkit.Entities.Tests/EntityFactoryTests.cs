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
		public void Can_create_simple_entity()
		{
			Assert.True(_entityFactory.TryCreate("template-simple", out var entity));
			
			var fetchedFoobar = entity.Get<Foobar>();
			Assert.Equal(123, fetchedFoobar.NumProperty);
			Assert.Equal("abcdef", fetchedFoobar.StringProperty);
		}

		[Fact]
		public void Can_create_complex_entity()
		{
			Assert.True(_entityFactory.TryCreate("template-with-embedded", out var entity));

			//sanity check
			var fetchedFoobar = entity.Get<Foobar>();
			Assert.Equal(123, fetchedFoobar.NumProperty);
			Assert.Equal("abcdef", fetchedFoobar.StringProperty);

		}
	}
}
