using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RoguelikeToolkit.Common.EntityTemplates
{
    public class EntityTemplateRepository
    {
        public readonly Dictionary<string, EntityTemplate> Templates = 
            new Dictionary<string, EntityTemplate>(
                Enumerable.Empty<KeyValuePair<string, EntityTemplate>>(), 
                StringComparer.InvariantCultureIgnoreCase);

        public EntityTemplateRepository(params string[] templateFolders)
        {
            foreach (var dir in templateFolders ?? Enumerable.Empty<string>())
                foreach(var jsonFile in Directory.EnumerateFiles(dir,"*.json", SearchOption.AllDirectories))
                    LoadTemplate(jsonFile);

            foreach (var template in Templates) 
                ProcessInheritance(template.Value);
        }

        private void LoadTemplate(string jsonFile)
        {
            var template = EntityTemplate.LoadFromFile(File.OpenRead(jsonFile));
            if (template != null && !string.IsNullOrWhiteSpace(template.Id))
                Templates.TryAdd(template.Id, template); //Do not override existing template. TODO: Add logging!
        }

        private void ProcessInheritance(EntityTemplate template)
        {
            //first, gather all instances of templates, both global and local
            //note: local template is one that is defined implicitly in the template
            //and doesn't exist in template files
            var mergedTemplateCollection = new Dictionary<string, EntityTemplate>(Templates, StringComparer.InvariantCultureIgnoreCase);
            MergeNested(mergedTemplateCollection, template);

            //now recursively fill in 'inherited' collection of each template
            foreach (var (templateId, templateToProcess) in mergedTemplateCollection.Where(t => !t.Value.IsInheritanceInitialized))
            {
                templateToProcess.IsInheritanceInitialized = true;
                var inheritedComponents = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);

                //figure out inherited components and add them as needed, overwriting those from base templates with those from inherited
                VisitInheritanceHierarchy(templateId, 
                    currentTemplate =>
                    {
                        if (currentTemplate.Id == templateId) 
                            return;

                        templateToProcess.InheritsFrom.Add(currentTemplate.Id);
                        foreach (var (key, value) in currentTemplate.Components)
                            inheritedComponents.AddOrReplace(key, (object)value);
                    });

                foreach (var (key, value) in inheritedComponents)
                    if (!templateToProcess.Components.ContainsKey(key))
                        templateToProcess.Components.Add(key, value);
            }

            //use DFS so inherited template components will hide the base definitions
            void VisitInheritanceHierarchy(string currentTemplateId, Action<EntityTemplate> visitor)
            {
                var currentTemplate = mergedTemplateCollection[currentTemplateId];
                foreach (var baseTemplateId in currentTemplate.InheritsFrom.ToArray())
                    VisitInheritanceHierarchy(baseTemplateId, visitor);

                visitor(currentTemplate);
            }

            void MergeNested(Dictionary<string, EntityTemplate> dest, EntityTemplate currentTemplate)
            {
                foreach (var childTemplate in currentTemplate.Children)
                {
                    //TODO: add warning if failed to add because of already existing template
                    dest.TryAdd(childTemplate.Key, childTemplate.Value);
                    MergeNested(dest, childTemplate.Value);
                }
            }
        }


    }
}
