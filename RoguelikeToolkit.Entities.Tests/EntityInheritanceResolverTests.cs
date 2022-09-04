using System;
using RoguelikeToolkit.Entities.Factory;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace RoguelikeToolkit.Entities.Tests
{
	public class EntityInheritanceResolverTests
	{
		private readonly EntityTemplateRepository _repository = new();
		private readonly EntityInheritanceResolver _inheritanceResolver;

		public EntityInheritanceResolverTests()
		{
			_repository.LoadTemplateFolder("TemplatesForInheritanceResolver");
			_inheritanceResolver = new(_repository.TryGetByName);
		}

		[Fact]
		public void Should_throw_on_non_existing_inherit_entries()
		{
			//sanity check
			Assert.True(_repository.TryGetByName("inherited-template-non-existing-inherit", out var template));
			Assert.Throws<InvalidOperationException>(() => _inheritanceResolver.GetEffectiveTemplate(template));
		}

		[Fact]
		public void Can_resolve_without_inheritance()
		{
			//sanity check
			Assert.True(_repository.TryGetByName("template-no-inheritance", out var template));
			var templateWithInheritanceResolved = _inheritanceResolver.GetEffectiveTemplate(template);

			Assert.Collection(templateWithInheritanceResolved.Tags,
				item => Assert.Equal("tag1", item),
				item => Assert.Equal("tag2", item),
				item => Assert.Equal("tag3", item));

			Assert.Collection(templateWithInheritanceResolved.Components,
				kvp =>
				{
					Assert.Equal("foobar", kvp.Key);

					//embedded objects yaml deserializer loads as Dictionary<object, object>
					var valueAsDict = (Dictionary<object, object>)kvp.Value;
					Assert.Equal("abcdef", valueAsDict["stringProperty"]);
					Assert.Equal((byte)123, valueAsDict["numProperty"]);
				},
				kvp =>
				{
					Assert.Equal("barfoo", kvp.Key);
					var valueAsDict = (Dictionary<object, object>)kvp.Value;
					Assert.Equal("defgh", valueAsDict["anotherStringProperty"]);
					Assert.Equal((byte)234, valueAsDict["anotherNumProperty"]);

				},
				kvp =>
				{
					//yaml deserializer loads simple objects as key-value pairs
					Assert.Equal("foo", kvp.Key);
					Assert.Equal("this is a test!", kvp.Value);
				});
		}

		[Fact]
		public void Can_resolve_multilevel_inheritance()
		{
			//sanity check
			Assert.True(_repository.TryGetByName("inherited-template-level2", out var template));

			var templateWithInheritanceResolved = _inheritanceResolver.GetEffectiveTemplate(template);

			Assert.NotNull(templateWithInheritanceResolved); //sanity check

			Assert.Collection(templateWithInheritanceResolved.Tags.OrderBy(x => x),
				tag => Assert.Equal("tag1", tag),
				tag => Assert.Equal("tag2", tag),
				tag => Assert.Equal("tag3", tag),
				tag => Assert.Equal("tag4", tag));

			Assert.Collection(templateWithInheritanceResolved.Components.OrderBy(x => x.Key),
				kvp =>
				{
					//yaml deserializer loads simple objects as key-value pairs
					Assert.Equal("bar", kvp.Key);
					Assert.Equal("this is also a test!", kvp.Value);
				},
				kvp =>
				{
					Assert.Equal("barfoo", kvp.Key);
					var valueAsDict = (Dictionary<object, object>)kvp.Value;
					Assert.Equal("defgh", valueAsDict["anotherStringProperty"]);
					Assert.Equal((byte)234, valueAsDict["anotherNumProperty"]);

				},
				kvp =>
				{
					//yaml deserializer loads simple objects as key-value pairs
					Assert.Equal("foo", kvp.Key);
					Assert.Equal("this is a test!", kvp.Value);
				},
				kvp =>
				{
					Assert.Equal("foobar", kvp.Key);

					//embedded objects yaml deserializer loads as Dictionary<object, object>
					var valueAsDict = (Dictionary<object, object>)kvp.Value;
					Assert.Equal("abcdef", valueAsDict["stringProperty"]);
					Assert.Equal((byte)123, valueAsDict["numProperty"]);
				}
			);
		}
	}
}
