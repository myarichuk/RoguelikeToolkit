using RoguelikeToolkit.Entities.Components;
using RoguelikeToolkit.Entities.Factory;
using Xunit;

namespace RoguelikeToolkit.Entities.Tests
{
	[Component]
	public class ComponentA
	{
	}

	[Component]
	public struct ComponentB
	{
	}

	public class ComponentC: IValueComponent<string>
	{
		public string Value { get; set; }
	}

	public class ComponentTypeRegistryTests
	{
		private readonly ComponentTypeRegistry _componentTypeRegistry = new();

		[Fact]
		public void Can_fetch_component_type_with_attribute()
		{
			Assert.True(_componentTypeRegistry.TryGetComponentType(nameof(ComponentA), out _));
			Assert.True(_componentTypeRegistry.TryGetComponentType(nameof(ComponentB), out _));
		}

		[Fact]
		public void Can_fetch_value_component_from_the_same_assembly() =>
			Assert.True(_componentTypeRegistry.TryGetComponentType(nameof(ComponentC), out _));

		[Fact]
		public void Can_fetch_value_component_from_the_another_assembly() =>
			//component from another assembly
			Assert.True(_componentTypeRegistry.TryGetComponentType(nameof(TagsComponent), out _));

		[Fact]
		public void Will_return_false_on_non_existing_component_type() =>
			Assert.False(_componentTypeRegistry.TryGetComponentType("non-existing-type", out _));
	}
}
