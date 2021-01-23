using System;
using System.Collections;
using System.Collections.Generic;

namespace RoguelikeToolkit.KeyMapping
{
    public class KeyCombination<KeyEnum> : IEnumerable<KeyEnum>
        where KeyEnum : Enum
    {
        private readonly static IEqualityComparer<HashSet<KeyEnum>> _comparer;

        static KeyCombination() => _comparer = HashSet<KeyEnum>.CreateSetComparer();

        internal KeyCombination() { }

        public KeyCombination(string id, params KeyEnum[] keys)
        {
            Keys = new HashSet<KeyEnum>(keys);
            Id = id;
        }

        public override bool Equals(object obj) =>
            obj is KeyCombination<KeyEnum> combination && _comparer.Equals(combination.Keys);

        public string Id { get; internal set; }

        public HashSet<KeyEnum> Keys { get; internal set; }

        public IEnumerator<KeyEnum> GetEnumerator() => Keys.GetEnumerator();

        public override int GetHashCode()
        {
            var hashCode = -576119966;
            foreach (var key in Keys)
                hashCode = HashCode.Combine(hashCode, key);
            return hashCode;
        }

        IEnumerator IEnumerable.GetEnumerator() => Keys.GetEnumerator();
    }
}
