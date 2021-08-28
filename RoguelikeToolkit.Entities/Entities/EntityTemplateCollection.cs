using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace RoguelikeToolkit.Entities.Entities
{
    public class EntityTemplateCollection
    {
        private readonly IReadOnlyDictionary<string, EntityTemplate> _templates;

        public EntityTemplateCollection(params string[] templateFolders)
        {
            Dictionary<string, EntityTemplate> templates = new(StringComparer.InvariantCultureIgnoreCase);

            foreach (var folder in templateFolders)
                LoadTemplateFolder(folder, templates);

            _templates = templates; //not strictly necessary to have read-only dictionary as field, but might be useful later
        }

        public IEnumerable<EntityTemplate> Templates => _templates.Values;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetById(string id, out EntityTemplate template) => _templates.TryGetValue(id, out template);

        #region Helpers

        private static void LoadTemplateFolder(string folder, Dictionary<string, EntityTemplate> templates)
        {
            if (!Directory.Exists(folder))
                throw new DirectoryNotFoundException($"'{folder}' couldn't be found.");

            var allTemplateCandidates = Directory.EnumerateFiles(folder, "*.json", SearchOption.AllDirectories);

            foreach (var jsonPath in allTemplateCandidates)
            {
                try
                {
                    var template = EntityTemplate.ParseFromFile(jsonPath);

                    if (templates.ContainsKey(template.Id))
                        throw new InvalidDataException($"Found duplicate templates for Id = {template.Id}, the duplicate template file is {jsonPath}");

                    templates.Add(template.Id, template);
                }
                catch (InvalidDataException ex)
                {
                    throw new InvalidOperationException($"Found json file (path = '{jsonPath}') incompatible with Entity Template schema. See inner exception for more details.", ex);
                }
            }
        }

        #endregion
    }
}
