using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RoguelikeToolkit.Common.EntityTemplates
{
    public class EntityTemplateCollection
    {
  

        public readonly Dictionary<string, EntityTemplate> Templates = 
            new Dictionary<string, EntityTemplate>(
                Enumerable.Empty<KeyValuePair<string, EntityTemplate>>(), 
                StringComparer.InvariantCultureIgnoreCase);

        public EntityTemplateCollection(params string[] templateFolders)
        {
            foreach (var dir in templateFolders ?? Enumerable.Empty<string>())
                foreach(var jsonFile in Directory.EnumerateFiles(dir,"*.json", SearchOption.AllDirectories))
                    LoadTemplate(jsonFile);


            void ProcessComponentInheritance(EntityTemplate template)
            {
                if(template.Children?.Count > 0)
                    //note, the 'where' is precaution, this should never be the case
                    //TODO: add warning for 'null' child templates
                    foreach (var childTemplate in template.Children.Where(c => c.Value != null))
                    {
                        childTemplate.Value.InheritsFrom = GatherInheritance(childTemplate).Distinct().ToArray();

                        foreach (var inheritedTemplateId in childTemplate.Value.InheritsFrom)
                        foreach (var component in Templates[inheritedTemplateId].Components)
                        {
                            //TODO: add logging if component already exists
                            childTemplate.Value.Components.TryAdd(component.Key, component.Value);
                        }

                        IEnumerable<string> GatherInheritance(KeyValuePair<string, EntityTemplate> templateInfo)
                        {
                            foreach (var inheritedTemplateId in templateInfo.Value.InheritsFrom)
                            {
                                if (Templates.ContainsKey(inheritedTemplateId))
                                {
                                    foreach (var subInheritedTemplateId in
                                        GatherInheritance(
                                            new KeyValuePair<string, EntityTemplate>(
                                                inheritedTemplateId, Templates[inheritedTemplateId])))
                                    {
                                        if (Templates.ContainsKey(subInheritedTemplateId))
                                            yield return subInheritedTemplateId;
                                    }

                                }
                                yield return inheritedTemplateId;
                            }

                            if (Templates.ContainsKey(templateInfo.Key))
                            {
                                foreach (var inheritedTemplateId in Templates[templateInfo.Key].InheritsFrom)
                                {
                                    if (Templates.ContainsKey(inheritedTemplateId))
                                        yield return inheritedTemplateId;

                                    foreach (var subInheritedTemplateId in
                                        GatherInheritance(
                                            new KeyValuePair<string, EntityTemplate>(
                                                inheritedTemplateId, Templates[inheritedTemplateId])))
                                    {
                                        if (Templates.ContainsKey(subInheritedTemplateId))
                                            yield return subInheritedTemplateId;
                                    }
                                }
                            }
                        }

                        ProcessComponentInheritance(childTemplate.Value);
                    }
            }

            foreach (var template in Templates) 
                ProcessComponentInheritance(template.Value);
        }

        private void LoadTemplate(string jsonFile)
        {
            var template = EntityTemplate.LoadFromFile(File.OpenRead(jsonFile));
            if (template != null && !string.IsNullOrWhiteSpace(template.Id))
                Templates.TryAdd(template.Id, template); //Do not override existing template. TODO: Add logging!
        }
    }
}
