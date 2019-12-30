using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace RoguelikeToolkit.Entities
{
    public class EntityTemplateCollection
    {
        private readonly Dictionary<string, EntityTemplate> _loadedTemplates = new Dictionary<string, EntityTemplate>(StringComparer.InvariantCultureIgnoreCase);

        public EntityTemplateCollection(params string[] templateFolders)
        {
            var currentAssembly = Assembly.GetExecutingAssembly();
            var embeddedTemplateResourceNames = currentAssembly.GetManifestResourceNames().Where(n => n.EndsWith(".json"));

            foreach (var embeddedResourceName in embeddedTemplateResourceNames)
            {
                using var stream = currentAssembly.GetManifestResourceStream(embeddedResourceName);

                if (stream == null) //just in case, should never happen
                    continue;

                using var reader = new StreamReader(stream);
                var template = EntityTemplate.Parse(reader.ReadToEnd());
                _loadedTemplates.Add(template.Id ?? 
                                        throw new InvalidDataException($"Error loading embedded template '{embeddedResourceName}'. Expected for its ID to be non null, but it wasn't"), 
                                     template);
            }

            foreach(var folder in templateFolders.Where(Directory.Exists))
                foreach(var templateFilename in Directory.EnumerateFiles(folder, "*.json", SearchOption.AllDirectories))
                    LoadTemplate(templateFilename);

            AddChildTemplatesToLoadedList();
            foreach (var template in _loadedTemplates)
                AddChildInheritedComponents(template.Value);
        }

        private void AddChildInheritedComponents(EntityTemplate template)
        {
            foreach(var child in template.Children)
            {
                AddInheritedComponents(child.Value);
                AddChildInheritedComponents(child.Value);
            }
        }

        private void AddInheritedComponents(EntityTemplate template)
        {
            foreach(var component in GetInheritedComponents(template))
            {
                if (template.Components.ContainsKey(component.Key))
                    continue;
                template.Components.Add(component.Key, component.Value);
            }
        }

        private IEnumerable<KeyValuePair<string, object>> GetInheritedComponents(EntityTemplate template)
        {
            foreach(var inherited in template.Inherits)
            {
                if(_loadedTemplates.TryGetValue(inherited, out var inheritedTemplate))
                {
                    foreach (var c in inheritedTemplate.Components)
                        yield return c;
                    foreach (var inheritedC in GetInheritedComponents(inheritedTemplate))
                        yield return inheritedC;
                }
            }
        }

        private void AddChildTemplatesToLoadedList()
        {
            foreach (var template in _loadedTemplates.ToDictionary(x => x.Key, x => x.Value))
            {
                foreach (var childTemplate in template.Value.Children)
                {
                    if (!_loadedTemplates.ContainsKey(childTemplate.Key))
                        _loadedTemplates.Add(childTemplate.Key, childTemplate.Value);
                }
            }
        }

        public IReadOnlyDictionary<string, EntityTemplate> LoadedTemplates => _loadedTemplates;

        private void LoadTemplate(string filename)
        {
            if(!File.Exists(filename))
                throw new FileNotFoundException("Couldn't find template file.", filename);

            var template = EntityTemplate.Parse(File.ReadAllText(filename));

            #if NETSTANDARD2_1
            _loadedTemplates.TryAdd(template.Id, template);
            #else
            if(!_loadedTemplates.ContainsKey(template.Id))
                _loadedTemplates.Add(template.Id, template);
            #endif
        }

    }
}
