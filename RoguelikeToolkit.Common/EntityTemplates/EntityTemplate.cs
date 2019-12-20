using System;
using System.Collections.Generic;
using System.IO;
using System.Json;
using System.Linq;
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

        public string[] InheritsFrom; //parent template names

        [DataMember(Name = nameof(Components), IsRequired = true)]
        public Dictionary<string, dynamic> Components = 
            new Dictionary<string, dynamic>(
                Enumerable.Empty<KeyValuePair<string, dynamic>>(), 
                StringComparer.InvariantCultureIgnoreCase);

        public static EntityTemplate LoadFromJson(string filePath)
        {
            using var fs = File.OpenRead(filePath);
            using var reader = new StreamReader(fs);

            return JsonSerializer.Deserialize<EntityTemplate>(reader.ReadToEnd());
        }
    }
}
