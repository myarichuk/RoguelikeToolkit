using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace RoguelikeToolkit.Entities
{
    public class EffectiveEntityTemplate : IEntityTemplate
    {
        private readonly EntityTemplate _root;
        private readonly IEnumerable<EntityTemplate> _inheritanceChain;
        private readonly Lazy<Dictionary<string, ComponentTemplate>> _components;
        private readonly Lazy<HashSet<string>> _inherits;
        private readonly Lazy<HashSet<string>> _tags;
        private readonly Lazy<Dictionary<string, EntityTemplate>> _childEntities;

        public EffectiveEntityTemplate(EntityTemplate template, IEnumerable<EntityTemplate> inheritanceChain)
        {
            _root = template;
            _inheritanceChain = inheritanceChain;
            _components = new Lazy<Dictionary<string, ComponentTemplate>>(
                () => GetEffectiveComponents().ToDictionary(x => x.Key, x => x.Value));

            _childEntities = new Lazy<Dictionary<string, EntityTemplate>>(
                () => GetEffectiveChildEntities().ToDictionary(x => x.Key, x => x.Value));

            _inherits = new Lazy<HashSet<string>>(() => new HashSet<string>(_inheritanceChain.SelectMany(x => x.Inherits)));
            _tags = new Lazy<HashSet<string>>(() => new HashSet<string>(_inheritanceChain.SelectMany(x => x.Tags)));
        }

        public Dictionary<string, ComponentTemplate> Components => _components.Value;
        public string Id => _root.Id;
        public HashSet<string> Inherits => _inherits.Value;
        public HashSet<string> Tags => _tags.Value;
        public Dictionary<string, EntityTemplate> ChildEntities => _childEntities.Value;

        #region Helpers

        //note: this method is likely to be inefficient
        //TODO: profile and perhaps optimize this
        private IEnumerable<KeyValuePair<string, TItem>> GetEffectiveItems<TItem>(Func<EntityTemplate, IDictionary<string, TItem>> itemExtractor)
        {
            var items = itemExtractor(_root).AsEnumerable();

            foreach (var template in _inheritanceChain)
            {
                items = items.Concat(itemExtractor(template));
            }

            var mergedItems = from kvp in items
                              group kvp by kvp.Key into g
                              select new KeyValuePair<string, TItem>(g.Key, g.First().Value);

            return mergedItems;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IEnumerable<KeyValuePair<string, EntityTemplate>> GetEffectiveChildEntities() =>
            GetEffectiveItems(template => template.ChildEntities);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IEnumerable<KeyValuePair<string, ComponentTemplate>> GetEffectiveComponents() =>
            GetEffectiveItems(template => template.Components);

        #endregion
    }
}
