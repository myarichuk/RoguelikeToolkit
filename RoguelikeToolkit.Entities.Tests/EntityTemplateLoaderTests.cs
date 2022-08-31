using System.Collections.Generic;
using System.IO;
using System.Security;
using Xunit;

namespace RoguelikeToolkit.Entities.Tests
{
	public class EntityTemplateLoaderTests
	{
		private readonly EntityTemplateLoader _loader = new();

		[Fact]
		public void Can_load_empty_yaml()
		{
			var template = _loader.LoadFrom(new FileInfo(Path.Combine("TemplatesForLoading", "empty.yaml")));
			Assert.NotNull(template);
			Assert.Null(template.Id);
			Assert.Empty(template.Inherits);
			Assert.Empty(template.ChildrenById);
		}

		[Fact]
		public void Can_load_empty_json()
		{
			var template = _loader.LoadFrom(new FileInfo(Path.Combine("TemplatesForLoading", "empty.json")));
			Assert.NotNull(template);
			Assert.Null(template.Id);
			Assert.Empty(template.Inherits);
			Assert.Empty(template.ChildrenById);
		}

		[Fact]
		public void Can_load_yaml_only_with_id()
		{
			var template = _loader.LoadFrom(new FileInfo(Path.Combine("TemplatesForLoading", "template-only-id.yaml")));

			Assert.NotNull(template);
			Assert.NotNull(template.Id);
			Assert.Equal("this is a value of Id property", template.Id);
			Assert.Empty(template.Inherits);
			Assert.Empty(template.ChildrenById);
		}

		[Fact]
		public void Can_load_json_only_with_id()
		{
			var template = _loader.LoadFrom(new FileInfo(Path.Combine("TemplatesForLoading", "template-only-id.json")));

			Assert.NotNull(template);
			Assert.NotNull(template.Id);
			Assert.Equal("this is a value of Id property", template.Id);
			Assert.Empty(template.Inherits);
			Assert.Empty(template.ChildrenById);
		}

		[Fact]
		public void Can_load_yaml_simple_template()
		{
			var template = _loader.LoadFrom(new FileInfo(Path.Combine("TemplatesForLoading", "template-simple-case-sensitive-props.yaml")));

			Assert.NotNull(template);
			Assert.NotNull(template.Id);
			Assert.Equal("FooBarId", template.Id);

			Assert.Collection(template.Tags,
				 item => Assert.Equal("tag1", item),
				 item => Assert.Equal("tag2", item),
				 item => Assert.Equal("tag3", item));

			Assert.Collection(template.Components,
				kvp =>
				{
					Assert.Equal("foobar", kvp.Key);
					var valueAsDict = (Dictionary<object, object>)kvp.Value;
					Assert.Equal("abcdef", valueAsDict["stringPropery"]);
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
					Assert.Equal("foo", kvp.Key);
					Assert.Equal("this is a test!", kvp.Value);
				});
		}
	}
}
