using RoguelikeToolkit.Entities.Repository;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;

namespace RoguelikeToolkit.Entities
{
	//TODO: add logging
	public class EntityTemplateRepository
	{
		private readonly EntityTemplateLoader _loader = new();
		private readonly ConcurrentDictionary<string, EntityTemplate> _entityRepository = new(StringComparer.InvariantCultureIgnoreCase);
		private static readonly HashSet<string> ValidExtensions = new() { ".yaml", ".json" };
		public IEnumerable<string> TemplateNames => _entityRepository.Keys;

		/// <exception cref="ArgumentNullException"><paramref name="templateName"/> is <see langword="null"/></exception>
		public bool TryGetByName(string templateName, out EntityTemplate template)
		{
			if (templateName == null)
				throw new ArgumentNullException(nameof(templateName));

			var hasFound = _entityRepository.TryGetValue(templateName, out template);

			if (hasFound && string.IsNullOrWhiteSpace(template.Name))
				template.Name = templateName;

			return hasFound;
		}

		/// <exception cref="ArgumentNullException"><paramref name="tags"/> or any of it's items is <see langword="null"/></exception>
		public IEnumerable<EntityTemplate> GetByTags(params string[] tags)
		{
			if (tags == null)
				throw new ArgumentNullException(nameof(tags));

			if(tags.Any(t => t == null))
				throw new ArgumentNullException(nameof(tags), "one or more of the tags is null, this is not supported");

			return _entityRepository.Values.Where(t => t.Tags.IsSupersetOf(tags));
		}


		/// <exception cref="TemplateAlreadyExistsException">Template with specified name already exists.</exception>
		/// <exception cref="OverflowException">The repository cache contains too many elements.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="templateName"/> is <see langword="null"/></exception>
		public void LoadTemplate(string templateName, StreamReader reader)
		{
			if (templateName == null)
				throw new ArgumentNullException(nameof(templateName));

			var template = _loader.LoadFrom(reader);
			if (!_entityRepository.TryAdd(templateName, template))
				throw new TemplateAlreadyExistsException(templateName);
		}

		/// <exception cref="FileNotFoundException">Template file not found</exception>
		/// <exception cref="DirectoryNotFoundException">The specified path of template file is invalid, such as being on an unmapped drive.</exception>
		/// <exception cref="IOException">The template file is already open.</exception>
		/// <exception cref="UnauthorizedAccessException"><see cref="P:System.IO.FileInfo.Name" /> template file is read-only or is a directory.</exception>
		/// <exception cref="InvalidOperationException">Template files must have either 'yaml' or 'json' extensions</exception>
		/// <exception cref="OutOfMemoryException">The length of the one of the strings overflows the maximum allowed length (<see cref="System.Int32.MaxValue" />). This is highly unlikely but still can happen :)</exception>
		/// <exception cref="ArgumentNullException"><paramref name="templateFile"/> is <see langword="null"/></exception>
		/// <exception cref="OverflowException">The repository cache contains too many elements.</exception>
		/// <exception cref="TemplateAlreadyExistsException">Template with specified name already exists.</exception>
		public void LoadTemplate(FileInfo templateFile)
		{
			if (templateFile == null)
				throw new ArgumentNullException(nameof(templateFile));

			if (!templateFile.Exists)
				throw new FileNotFoundException("Template file not found", templateFile.FullName);

			if (ValidExtensions.Contains(templateFile.Extension))
				throw new InvalidOperationException($"Template files must have either {string.Join("or", ValidExtensions)} extensions");

			using var fs = templateFile.OpenRead();
			using var reader = new StreamReader(fs);

			var dot = templateFile.Name.LastIndexOf('.');
			LoadTemplate(templateFile.Name[..dot], reader);
		}


		/// <exception cref="ArgumentNullException"><paramref name="templateFilename"/> is <see langword="null"/></exception>
		/// <exception cref="SecurityException">The caller does not have the required permission to access template filename</exception>
		/// <exception cref="UnauthorizedAccessException">Access to file specified by templateFilename is denied.</exception>
		/// <exception cref="IOException">The template file is already open.</exception>
		/// <exception cref="FileNotFoundException">Template file not found</exception>
		/// <exception cref="DirectoryNotFoundException">The specified path of template file is invalid, such as being on an unmapped drive.</exception>
		/// <exception cref="OutOfMemoryException">The length of the one of the strings overflows the maximum allowed length (<see cref="System.Int32.MaxValue" />). This is highly unlikely but still can happen :)</exception>
		/// <exception cref="OverflowException">The repository cache contains too many elements.</exception>
		/// <exception cref="TemplateAlreadyExistsException">Template with specified name already exists.</exception>
		/// <exception cref="InvalidOperationException">Template files must have either 'yaml' or 'json' extensions</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void LoadTemplate(string templateFilename)
		{
			if (templateFilename == null) throw new ArgumentNullException(nameof(templateFilename));
			LoadTemplate(new FileInfo(templateFilename));
		}

		/// <exception cref="SecurityException">The caller does not have the required permission for the repository folder.</exception>
		/// <exception cref="PathTooLongException">The specified path of the repository folder exceeds the system-defined maximum length.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="templateFolder"/> is <see langword="null"/></exception>
		/// <exception cref="DirectoryNotFoundException">The specified path of template file is invalid, such as being on an unmapped drive.</exception>
		/// <exception cref="FileNotFoundException">Template file not found</exception>
		/// <exception cref="IOException">The template file is already open.</exception>
		/// <exception cref="UnauthorizedAccessException"><see cref="P:System.IO.FileInfo.Name" /> template file is read-only or is a directory.</exception>
		/// <exception cref="OutOfMemoryException">The length of the one of the strings overflows the maximum allowed length (<see cref="System.Int32.MaxValue" />). This is highly unlikely but still can happen :)</exception>
		/// <exception cref="OverflowException">The repository cache contains too many elements.</exception>
		/// <exception cref="TemplateAlreadyExistsException">Template with specified name already exists.</exception>
		/// <exception cref="ArgumentException">If .NET Framework and .NET Core versions older than 2.1: <paramref name="path" /> contains invalid characters such as ", &lt;, &gt;, or |.</exception>
		public void LoadTemplateFolder(string templateFolder)
		{
			if (templateFolder == null)
				throw new ArgumentNullException(nameof(templateFolder));

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
