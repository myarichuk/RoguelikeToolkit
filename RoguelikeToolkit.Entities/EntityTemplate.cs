﻿using System;
using System.Collections.Generic;
using System.IO;

namespace RoguelikeToolkit.Entities
{
    public class EntityTemplate
    {
        private const string InheritsMalformedMessage = "'Inherits' property is malformed - it should be a collection of strings, no more, no less!";
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
                ParseInheritsField(template, inherits);

            if(data.TryGetValue(nameof(Components), out var components))
                ParseComponentsField(template, components);

            return template;
        }

        private static void ParseComponentsField(EntityTemplate template, object components)
        {
            if (components is IDictionary<string, object> componentsStronglyTyped)
            {
                //TODO: finish implementing this
                foreach (var componentKV in componentsStronglyTyped)
                {
                    if (componentKV.Value is IDictionary<string, object> componentProps)
                        template.Components.AddOrSet(componentKV.Key, _ => new ComponentTemplate(componentProps));
                    else if (componentKV.Value.GetType().IsPrimitive)
                    {
                        //TODO: add IValueComponent<T> for the value...
                    }
                }
            }
            else
            {
                throw new InvalidDataException(ComponentsMalformedMessage);
            }
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