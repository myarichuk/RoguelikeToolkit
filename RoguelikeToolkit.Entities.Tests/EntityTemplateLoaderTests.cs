using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
	}
}
