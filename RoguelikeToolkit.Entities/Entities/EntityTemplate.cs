using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace RoguelikeToolkit.Entities
{
    public class EntityTemplate : IEquatable<EntityTemplate>, IEntityTemplate
    {
        private static readonly string[] EntityProperties =
        {
            nameof(Id),
            nameof(Components),
            nameof(Inherits),
            nameof(Tags)
        };

        private const string StringCollectionMalformed = "The property is malformed - it should be a collection of strings";
        private const string IdMissingMessage = "Missing required field -> 'Id'";
        private const string ComponentsMalformedMessage = "'Components' property is malformed - it should be an object where each field is a component";

        public string Id { get; private set; }
        public Dictionary<string, ComponentTemplate> Components { get; } = new Dictionary<string, ComponentTemplate>(StringComparer.InvariantCultureIgnoreCase);
        public HashSet<string> Inherits { get; } = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        public HashSet<string> Tags { get; } = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        public Dictionary<string, EntityTemplate> ChildEntities { get; } = new Dictionary<string, EntityTemplate>(StringComparer.InvariantCultureIgnoreCase);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EntityTemplate ParseFromFile(string jsonFile) =>
            ParseFromString(File.ReadAllText(jsonFile));

        public static EntityTemplate ParseFromString(string json)
        {
            if (!json.TryDeserialize(out var data))
                throw new InvalidDataException("Failed to parse malformed json");

            return ParseFromDictionary(data);
        }

        public static EntityTemplate ParseFromDictionary(IDictionary<string, object> data, string templateId = null)
        {
            var template = new EntityTemplate();

            if (templateId != null)
                template.Id = templateId;
            else
            {
                if (data.TryGetValue(nameof(Id), out var idAsObj) == false || !(idAsObj is string id))
                    throw new InvalidDataException(IdMissingMessage);
                else
                    template.Id = id;
            }

            if (data.TryGetValue(nameof(Inherits), out var inherits))
                ParseStringArrayField(inherits, template.Inherits);

            if (data.TryGetValue(nameof(Components), out var components))
                ParseComponentsField(template, components);

            if (data.TryGetValue(nameof(Tags), out var tags))
                ParseStringArrayField(tags, template.Tags);

            var embeddedTemplateNames = data.Keys.Except(EntityProperties);
            foreach (var templateName in embeddedTemplateNames)
            {
                //ensure unique name
                var embeddedTemplateId = $"{template.Id}.{templateName}";

                if (!(data[templateName] is IDictionary<string, object> templateData))
                    throw new InvalidDataException($"Embedded template (key = {templateName}) is expected to be json embedded object, but it has the type = {data[templateName].GetType().FullName}");

                var embeddedTemplate = ParseFromDictionary(templateData, embeddedTemplateId);
                template.ChildEntities.AddOrSet(templateName, _ => embeddedTemplate);
            }

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj) =>
            Equals(obj as EntityTemplate);

        public bool Equals(EntityTemplate other) =>
            other != null &&
            Id == other.Id;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => 2108858624 + EqualityComparer<string>.Default.GetHashCode(Id);

        public static bool operator ==(EntityTemplate left, EntityTemplate right) => EqualityComparer<EntityTemplate>.Default.Equals(left, right);

        public static bool operator !=(EntityTemplate left, EntityTemplate right) => !(left == right);

        #endregion
    }
}
