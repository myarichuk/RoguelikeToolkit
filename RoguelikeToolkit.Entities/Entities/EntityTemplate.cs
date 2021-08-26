using System;
using System.Collections.Generic;
using System.IO;

namespace RoguelikeToolkit.Entities
{
    public class EntityTemplate
    {
        private const string StringCollectionMalformed = "The property is malformed - it should be a collection of strings";
        private const string IdMissingMessage = "Missing required field -> 'Id'";
        private const string ComponentsMalformedMessage = "'Components' property is malformed - it should be an object where each field is a component";

        public string Id { get; private set; }
        public Dictionary<string, ComponentTemplate> Components { get; } = new Dictionary<string, ComponentTemplate>(StringComparer.InvariantCultureIgnoreCase);
        public HashSet<string> Inherits { get; } = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        public HashSet<string> Tags { get; } = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

        public static EntityTemplate ParseFromString(string json)
        {
            var template = new EntityTemplate();
            if (!json.TryDeserialize(out var data))
                throw new InvalidDataException("Failed to parse malformed json");

            if (data.ContainsKey(nameof(Id)) == false)
                throw new InvalidDataException(IdMissingMessage);

            if(data.TryGetValue(nameof(Inherits), out var inherits))
                ParseStringArrayField(inherits, template.Inherits);

            if(data.TryGetValue(nameof(Components), out var components))
                ParseComponentsField(template, components);

            if (data.TryGetValue(nameof(Tags), out var tags))
                ParseStringArrayField(tags, template.Tags);

            return template;
        }

        #region Helpers

        private static void ParseComponentsField(EntityTemplate template, object components)
        {
            if (components is IDictionary<string, object> componentsStronglyTyped)
            {
                foreach (var componentKV in componentsStronglyTyped)
                {
                    if (componentKV.Value is IDictionary<string, object> componentProps)
                        template.Components.AddOrSet(componentKV.Key, _ => new ComponentTemplate(componentProps));
                    else if (componentKV.Value is string || componentKV.Value.GetType().IsPrimitive)
                    {
                        template.Components.AddOrSet(componentKV.Key, _ => 
                            new ComponentTemplate(new Dictionary<string, object> { { "Value", componentKV.Value } }));
                    }
                    else
                    {
                        throw new InvalidDataException("Invalid component, it must be either a primitive value or an object");
                    }
                }
            }
            else
            {
                throw new InvalidDataException(ComponentsMalformedMessage);
            }
        }

        private static void ParseStringArrayField(object valuesList, ICollection<string> destination)
        {
            if (!(valuesList is List<object> valuesAsObjectList))
                throw new InvalidDataException(StringCollectionMalformed);

            for (int i = 0; i < valuesAsObjectList.Count; i++)
            {
                object valAsObject = valuesAsObjectList[i];
                if (valAsObject is string valAsString)
                    destination.Add(valAsString);
                else
                    throw new InvalidDataException(StringCollectionMalformed);
            }
        }

        #endregion
    }
}
