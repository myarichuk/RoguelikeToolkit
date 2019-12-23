using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Utf8Json;
using Utf8Json.Resolvers;

namespace RoguelikeToolkit.Common.EntityTemplates
{
    public class EntityTemplate
    {
        //template name, pretty much its id
        //only root templates have 'names' - if this is null, the template is 'anonymous'
        [DataMember(Name = nameof(Id), IsRequired = true)]
        public string Id; 

        //Ids of templates it inherits from
        [DataMember(Name = nameof(InheritsFrom))]
        public HashSet<string> InheritsFrom; //parent template names

        [DataMember(Name = nameof(Components), IsRequired = true)]
        public Dictionary<string, dynamic> Components = 
            new Dictionary<string, dynamic>(
                Enumerable.Empty<KeyValuePair<string, dynamic>>(), 
                StringComparer.InvariantCultureIgnoreCase);

        [IgnoreDataMember]
        public Dictionary<string, EntityTemplate> Children = 
            new Dictionary<string, EntityTemplate>(Enumerable.Empty<KeyValuePair<string, EntityTemplate>>(), 
                StringComparer.InvariantCultureIgnoreCase);

        [IgnoreDataMember]
        public bool IsInheritanceInitialized { get; set; }

        public static EntityTemplate LoadFromFile(FileStream fs)
        {
            using var reader = new StreamReader(fs);
            var template = LoadFromJson(reader.ReadToEnd());
            if (template == null) //not a template..
                return null;

            if(template.InheritsFrom == null)
                template.InheritsFrom = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

            return template;
        }


        public enum Traversal
        {
            DFS,
            BFS
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void VisitChildren<TState>(TState state, Action<EntityTemplate, TState> visitor, Traversal traversal = Traversal.DFS)
            => VisitChildren(this, state, visitor);

        private void VisitChildren<TState>(EntityTemplate currentTemplate, TState state, Action<EntityTemplate, TState> visitor, Traversal traversal = Traversal.DFS)
        {
            if (traversal == Traversal.DFS)
            {
                foreach (var childTemplate in currentTemplate.Children.Values)
                    VisitChildren(childTemplate, state, visitor);

                visitor(currentTemplate, state);
            }
            else
            {
                visitor(currentTemplate, state);
                foreach (var childTemplate in currentTemplate.Children.Values)
                    VisitChildren(childTemplate, state, visitor);
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator EntityTemplate(Dictionary<string, object> templateData) => ParseTemplateFromDictionary(null, templateData);

        private static EntityTemplate ParseTemplateFromDictionary(string templateKey, Dictionary<string, object> templateData)
        {
            var template = new EntityTemplate();

            //TODO: consider either throwing exceptions or logging here
            //required field
            if (templateData.TryGetValue(nameof(Id), out var id) && id is string idAsString)
                template.Id = idAsString;
            else if (!string.IsNullOrWhiteSpace(templateKey))
                template.Id = templateKey;
            else
                return null;

            //required field
            if (!templateData.ContainsKey(nameof(Components)))
                template.Components = new Dictionary<string, dynamic>();

            foreach (var (key, value) in templateData)
            {
                switch (key)
                {
                    case nameof(Id):
                        continue;
                    case nameof(InheritsFrom):
                        template.InheritsFrom = new HashSet<string>(((IEnumerable) value).Cast<string>(), StringComparer.InvariantCultureIgnoreCase);
                        break;
                    case nameof(Components):
                        template.Components = (Dictionary<string, object>) value;
                        break;
                    default:
                        //note: because of implicit operator this is recursive call
                        template.Children.Add(key, ParseTemplateFromDictionary(key, (Dictionary<string, object>)value));
                        break;
                }
            }
            if(template.InheritsFrom == null)
                template.InheritsFrom = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

            return template;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EntityTemplate LoadFromJson(string json) => 
            (EntityTemplate)JsonSerializer.Deserialize<dynamic>(json);
    }
}
