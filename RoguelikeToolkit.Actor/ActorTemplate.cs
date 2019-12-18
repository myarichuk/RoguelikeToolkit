using System;
using System.Collections.Generic;
using System.Linq;

namespace RoguelikeToolkit.Actor
{
    public class ActorTemplate
    {
        public Dictionary<string, dynamic> TemplateFields;
        public List<ActorTemplate> Parents;

        public Dictionary<string, dynamic> Fields => ActorTemplate.GetFields(this).SelectMany(x => x).ToDictionary(x => x.Key, x => x.Value);

        public static IEnumerable<IEnumerable<KeyValuePair<string, dynamic>>> GetFields(ActorTemplate template)
        {
            yield return template.TemplateFields;
            foreach(var t in template.Parents)
                foreach (var fieldSet in GetFields(t))
                    yield return fieldSet;
        }
    }
}
