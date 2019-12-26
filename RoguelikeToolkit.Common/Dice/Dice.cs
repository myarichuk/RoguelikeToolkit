using System;

namespace RoguelikeToolkit.Common.Dice
{
    public readonly struct Dice : IEquatable<Dice>
    {
        public enum ModificationType
        {
            None,
            RemoveHigh,
            RemoveLow
        }

        public readonly int Die;
        public readonly int Pips;

        public readonly int Keep;

        public readonly ModificationType Modification;

        public Dice(int die, int pips, int keep = 0, ModificationType modificationType = ModificationType.None)
        {
            Pips = pips;
            Modification = modificationType;
            Keep = keep;
            Die = die;
        }

        public int Evaluate()
        {
            
            throw new NotImplementedException();
        }

        #region IEquatable<Dice> Implementation
        public bool Equals(Dice other) => Die == other.Die && Pips == other.Pips && Keep == other.Keep && Modification == other.Modification;

        public override bool Equals(object obj) => obj is Dice other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Die;
                hashCode = (hashCode * 397) ^ Pips;
                hashCode = (hashCode * 397) ^ Keep;
                hashCode = (hashCode * 397) ^ (int) Modification;
                return hashCode;
            }
        }

        public static bool operator ==(Dice left, Dice right) => left.Equals(right);

        public static bool operator !=(Dice left, Dice right) => !left.Equals(right);

        #endregion
    }
}
