using RoguelikeToolkit.Entities.Repository;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace RoguelikeToolkit.Entities
{
	//TODO: add logging
	public class EntityTemplateRepository
	{
		private readonly EntityTemplateLoader _loader = new EntityTemplateLoader();
		private readonly ConcurrentDictionary<string, EntityTemplate> _entityRepository = new(StringComparer.InvariantCultureIgnoreCase);

		public IEnumerable<string> TemplateNames => _entityRepository.Keys;

		public bool TryGetByName(string templateName, out EntityTemplate template)
		{
			var hasFound = _entityRepository.TryGetValue(templateName, out template);

			if (hasFound && string.IsNullOrWhiteSpace(template.Name))
				template.Name = templateName;

			return hasFound;
		}

		public IEnumerable<EntityTemplate> GetByTags(params string[] tags) =>
			_entityRepository.Values.Where(t => t.Tags.IsSupersetOf(tags));


		public void LoadTemplate(string templateName, StreamReader reader)
		{
			var template = _loader.LoadFrom(reader);

			if (!_entityRepository.TryAdd(templateName, template))
				throw new TemplateAlreadyExistsException(templateName);
		}

		public void LoadTemplate(FileInfo templateFile)
		{
			if (!templateFile.Exists)
				throw new FileNotFoundException("Template file not found", templateFile.FullName);

			if (templateFile.Extension != ".json" && templateFile.Extension != ".yaml")
				throw new InvalidOperationException("Template files must have either 'yaml' or 'json' extensions");

			using var fs = templateFile.OpenRead();
			using var reader = new StreamReader(fs);

			var dot = templateFile.Name.LastIndexOf('.');
			LoadTemplate(templateFile.Name[..dot], reader);
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void LoadTemplate(string templateFilename) =>
			LoadTemplate(new FileInfo(templateFilename));

		public void LoadTemplateFolder(string templateFolder)
		{
			var di = new DirectoryInfo(templateFolder);
			if (!di.Exists)
				throw new DirectoryNotFoundException($"Template directory not found (path = {templateFolder})");

			foreach (var fi in EnumerateTemplateFiles(di))
				LoadTemplate(fi);

			static IEnumerable<FileInfo> EnumerateTemplateFiles(DirectoryInfo di) =>
				di.EnumerateFiles("*.yaml", SearchOption.AllDirectories)
					.Concat(di.EnumerateFiles("*.json", SearchOption.AllDirectories));
		}
	}
}
