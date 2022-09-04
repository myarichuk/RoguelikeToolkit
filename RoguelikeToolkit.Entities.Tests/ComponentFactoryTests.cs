using System.Collections.Generic;
using RoguelikeToolkit.Entities.Factory;
using Xunit;

namespace RoguelikeToolkit.Entities.Tests
{
	public class ComponentFactoryTests
	{
		private readonly ComponentFactory _componentFactory = new();
		private readonly EntityTemplateRepository _repository = new();

		public ComponentFactoryTests() =>
			_repository.LoadTemplateFolder("TemplatesForComponentFactory");

		[Fact]
		public void Can_create_simple_class_component()
		{
			//sanity check
			Assert.True(_repository.TryGetByName("simple-template", out var template));
			var componentInstance =
				_componentFactory.CreateInstance<Foobar>(template.Components["foobar"] as Dictionary<object, object>);

			Assert.Equal(123, componentInstance.NumProperty);
			Assert.Equal("abcdef", componentInstance.StringProperty);
		}

		[Fact]
		public void Can_create_simple_class_component_with_missing_property()
		{
			//sanity check
			Assert.True(_repository.TryGetByName("simple-template", out var template));
			var componentInstance =
				_componentFactory.CreateInstance<PartialFoobar>(template.Components["foobar"] as Dictionary<object, object>);

			Assert.Equal("abcdef", componentInstance.StringProperty);
		}

		[Fact]
		public void Can_create_simple_class_component_with_missing_property2()
		{
			//sanity check
			Assert.True(_repository.TryGetByName("simple-template", out var template));
			var componentInstance =
				_componentFactory.CreateInstance<ComplexFoobar>(template.Components["foobar"] as Dictionary<object, object>);

			Assert.Equal("abcdef", componentInstance.StringProperty);
			Assert.Null(componentInstance.Embedded);
		}

		[Fact]
		public void Can_create_complex_class_component()
		{
			//sanity check
			Assert.True(_repository.TryGetByName("complex-template", out var template));
			var componentInstance =
				_componentFactory.CreateInstance<ComplexFoobar>(template.Components["foobar"] as Dictionary<object, object>);

			Assert.Equal(123, componentInstance.NumProperty);
			Assert.Equal("abcdef", componentInstance.StringProperty);

			Assert.Equal(234, componentInstance.Embedded.AnotherNumProperty);
			Assert.Equal("defgh", componentInstance.Embedded.AnotherStringProperty);
		}

		internal class Foobar
		{
			public int NumProperty { get; set; }
			public string StringProperty { get; set; }
		}

		internal class PartialFoobar
		{
			public string StringProperty { get; set; }
		}

		
		internal class ComplexFoobar
		{
			public int NumProperty { get; set; }
			public string StringProperty { get; set; }
			public BarFoo Embedded { get; set; }
		}

		internal class BarFoo
		{
			public string AnotherStringProperty { get; set; }
			public int AnotherNumProperty { get; set; }
		}

	}
}
