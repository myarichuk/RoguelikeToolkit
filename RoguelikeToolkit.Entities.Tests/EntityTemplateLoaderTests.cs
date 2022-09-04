using RoguelikeToolkit.Entities.Repository;
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
			Assert.Empty(template.Inherits);
		}

		[Fact]
		public void Can_load_empty_json()
		{
			var template = _loader.LoadFrom(new FileInfo(Path.Combine("TemplatesForLoading", "empty.json")));
			Assert.NotNull(template);
			Assert.Empty(template.Inherits);
		}


		[Theory]
		[InlineData("template-simple-case-sensitive-props-2.yaml")]
		[InlineData("template-simple-case-sensitive-props-1.json")]
		public void Can_load_simple_template(string templateFilename)
		{
			var template = _loader.LoadFrom(new FileInfo(Path.Combine("TemplatesForLoading", templateFilename)));

			Assert.NotNull(template);

			Assert.Collection(template.Tags,
				 item => Assert.Equal("tag1", item),
				 item => Assert.Equal("tag2", item),
				 item => Assert.Equal("tag3", item));

			Assert.Collection(template.Components,
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
	}
}
