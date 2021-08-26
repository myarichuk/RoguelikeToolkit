using System;
using System.Collections.Generic;
using System.IO;
using Utf8Json;

namespace RoguelikeToolkit.Entities
{
    public class EntityTemplate
    {
        private const string InheritsMalformedMessage = "'Inherits' property is malformed - it should be a collection of strings, no more, no less!";
        private const string IdMissingMessage = "Missing required field -> 'Id'";

        public string Id { get; private set; }
        
        public Dictionary<string, ComponentTemplate> Components { get; } = new Dictionary<string, ComponentTemplate>(StringComparer.InvariantCultureIgnoreCase);

        public HashSet<string> Inherits { get; } = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

        public HashSet<string> Tags { get; } = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

        public static EntityTemplate ParseFromString(string json)
        {
            var template = new EntityTemplate();
            var data = (IDictionary<string, object>)JsonSerializer.Deserialize<dynamic>(json);

            if (data.ContainsKey(nameof(Id)) == false)
                throw new InvalidDataException(IdMissingMessage);

            if(data.TryGetValue(nameof(Inherits), out object inherits))
                ParseInheritsField(template, inherits);

            if(data.TryGetValue(nameof(Components), out var components))
            {

            }

            return template;
        }

        private static void ParseInheritsField(EntityTemplate template, object inherits)
        {
            if (!(inherits is List<object> inheritsAsObjects))
                throw new InvalidDataException(InheritsMalformedMessage);

            for (int i = 0; i < inheritsAsObjects.Count; i++)
            {
                object idAsObject = inheritsAsObjects[i];
                if (idAsObject is string id)
                    template.Inherits.Add(id);
                else
                    throw new InvalidDataException(InheritsMalformedMessage);
            }
        }
    }
}
