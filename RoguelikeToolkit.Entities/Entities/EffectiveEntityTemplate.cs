using System;
using System.Collections.Generic;
using System.Linq;

namespace RoguelikeToolkit.Entities.Entities
{
    public class EffectiveEntityTemplate : IEntityTemplate
    {
        private readonly EntityTemplate _root;
        private readonly IEnumerable<EntityTemplate> _inheritanceChain;
        private readonly Lazy<Dictionary<string, ComponentTemplate>> _components;
        private readonly Lazy<HashSet<string>> _inherits;
        private readonly Lazy<HashSet<string>> _tags;

        public EffectiveEntityTemplate(EntityTemplate template, IEnumerable<EntityTemplate> inheritanceChain)
        {
            _root = template;
            _inheritanceChain = inheritanceChain;
            _components = new Lazy<Dictionary<string, ComponentTemplate>>(
                () => GetEffectiveComponents().ToDictionary(x => x.Key, x => x.Value));

            _inherits = new Lazy<HashSet<string>>(() => new HashSet<string>(_inheritanceChain.SelectMany(x => x.Inherits)));
            _tags = new Lazy<HashSet<string>>(() => new HashSet<string>(_inheritanceChain.SelectMany(x => x.Tags)));
        }

        public Dictionary<string, ComponentTemplate> Components => _components.Value;
        public string Id => _root.Id;
        public HashSet<string> Inherits => _inherits.Value;
        public HashSet<string> Tags => _tags.Value;

        #region Helpers

        private IEnumerable<KeyValuePair<string, ComponentTemplate>> GetEffectiveComponents()
        {
            var components = _root.Components.AsEnumerable();

            //inefficient, I know
            foreach (var template in _inheritanceChain)
                components = components.Concat(template.Components);

            //TODO: profile and perhaps optimize this
            var mergedComponents = from kvp in components
                                   group kvp by kvp.Key into g
                                   select new KeyValuePair<string, ComponentTemplate>(g.Key, g.First().Value);

            return mergedComponents;
        }

        #endregion
    }
}
