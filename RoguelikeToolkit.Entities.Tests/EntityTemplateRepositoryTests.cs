using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;
// ReSharper disable ExceptionNotDocumented

namespace RoguelikeToolkit.Entities.Tests
{
	public class EntityTemplateRepositoryTests
	{
		private readonly EntityTemplateRepository _repository = new();

		[Fact]
		public void Can_load_folder()
		{
			_repository.LoadTemplateFolder("FolderForTemplateRepository");
			Assert.Collection(_repository.TemplateNames.OrderBy(x => x),
				name => Assert.Equal("template-only-tags", name),
				name => Assert.Equal("template-only-tags.test.foobar", name),
				name => Assert.Equal("template-simple-case-sensitive-props-1", name),
				name => Assert.Equal("template-simple-case-sensitive-props-2", name),
				name => Assert.Equal("template-simple-case-sensitive-props-subfolder", name));
		}

		[Theory]
		[InlineData("template-only-tags.txt")]
		[InlineData("template-only-tags")]
		public void Should_fail_load_single_template_invalid_extension(string templateFilename)
		{
			string currentFolder = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;
			var path = Path.Combine(currentFolder, "FolderForTemplateRepository", templateFilename);
			Assert.Throws<InvalidOperationException>(() => _repository.LoadTemplate(path, false));
		}

		[Fact]
		public void Can_query_by_tags()
		{
			_repository.LoadTemplateFolder("FolderForTemplateRepository");

			Assert.Collection(_repository.GetByTags("tag1"),
				template => template.Tags.Contains("xyz"));

			Assert.Collection(_repository.GetByTags("tag3"),
				template => template.Tags.Contains("xyz"),
				template => template.Tags.Contains("aaa"));

			Assert.Collection(_repository.GetByTags("tag2"),
				template => template.Tags.Contains("aaa"));
		}

		[Fact]
		public void Can_load_single_template()
		{
			string currentFolder = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;
			var path = Path.Combine(currentFolder, "FolderForTemplateRepository", "template-only-tags.yaml");
			_repository.LoadTemplate(path);

			Assert.True(_repository.TryGetByName("template-only-tags", out _));
		}

		[Fact]
		public void Can_load_single_template_multiple_dots_in_filename()
		{
			string currentFolder = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;
			var path = Path.Combine(currentFolder, "FolderForTemplateRepository", "template-only-tags.test.foobar.yaml");
			_repository.LoadTemplate(path);

			Assert.True(_repository.TryGetByName("template-only-tags.test.foobar", out _));
		}
	}
}
