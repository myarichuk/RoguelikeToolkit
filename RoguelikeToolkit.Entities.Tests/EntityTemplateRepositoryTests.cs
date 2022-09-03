using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

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
				name => Assert.Equal("template-only-id", name),
				name => Assert.Equal("template-only-id.test.foobar", name),
				name => Assert.Equal("template-simple-case-sensitive-props-1", name),
				name => Assert.Equal("template-simple-case-sensitive-props-2", name));
		}

		[Theory]
		[InlineData("template-only-id.txt")]
		[InlineData("template-only-id")]
		public void Should_fail_load_single_template_invalid_extension(string templateFilename)
		{
			string currentFolder = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;
			var path = Path.Combine(currentFolder, "FolderForTemplateRepository", templateFilename);
			Assert.Throws<InvalidOperationException>(() => _repository.LoadTemplate(path));
		}

		[Fact]
		public void Can_load_single_template()
		{
			string currentFolder = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;
			var path = Path.Combine(currentFolder, "FolderForTemplateRepository", "template-only-id.yaml");
			_repository.LoadTemplate(path);

			Assert.True(_repository.TryGetByName("template-only-id", out _));
		}

		[Fact]
		public void Can_load_single_template_multiple_dots_in_filename()
		{
			string currentFolder = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;
			var path = Path.Combine(currentFolder, "FolderForTemplateRepository", "template-only-id.test.foobar.yaml");
			_repository.LoadTemplate(path);

			Assert.True(_repository.TryGetByName("template-only-id.test.foobar", out _));
		}
	}
}
