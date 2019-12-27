using System;
using System.Collections.Generic;

namespace RoguelikeToolkit.Entities
{
    public class EntityTemplate : IEquatable<EntityTemplate>
    {
        public string Id { get; set; }
        public HashSet<EntityTemplate> Children { get; set; }
        public Dictionary<string, object> Components { get; set; }

        #region IEquatable Implementation

        public bool Equals(EntityTemplate other) =>
            !ReferenceEquals(null, other) && (ReferenceEquals(this, other) ||
                                              string.Equals(Id, other.Id,
                                                  StringComparison.InvariantCultureIgnoreCase));

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;

            return obj.GetType() == this.GetType() && Equals((EntityTemplate) obj);
        }

        public override int GetHashCode() => (Id != null ? Id.GetHashCode() : 0);

        public static bool operator ==(EntityTemplate left, EntityTemplate right) => Equals(left, right);
        public static bool operator !=(EntityTemplate left, EntityTemplate right) => !Equals(left, right);

        #endregion
    }
}
