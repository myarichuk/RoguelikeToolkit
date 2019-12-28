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

        public EntityTemplateCollection()
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
        }

        public void LoadTemplate(string filename)
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
