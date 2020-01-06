﻿using System;
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

            foreach(var template in _loadedTemplates.OrderBy(x => x.Value.Inherits.Count))
                InitializeInheritChains(template.Value);
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

        public void InitializeInheritChains(EntityTemplate template)
        {
            foreach(var childTemplate in template.Children)
                InitializeInheritChains(childTemplate.Value);

            foreach (var inherits in template.Inherits)
            {
                if (!_loadedTemplates.TryGetValue(inherits, out var parentTemplate))
                    continue;
                InitializeInheritChains(parentTemplate);
                ApplyInheritance(template, parentTemplate);
            }
        }

        private void ApplyInheritance(EntityTemplate template, EntityTemplate parentTemplate)
        {
            foreach (var parentComponent in parentTemplate.Components)
            {
                //don't override existing components
                if(template.Components.ContainsKey(parentComponent.Key))
                    continue;

                template.Components.Add(parentComponent.Key, parentComponent.Value);
            }

            foreach (var childrenTemplate in parentTemplate.Children)
            {
                if(template.Children.ContainsKey(childrenTemplate.Key))
                    continue;

                template.Children.Add(childrenTemplate.Key, childrenTemplate.Value);
            }
        }
    }
}