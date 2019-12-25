using System;

namespace RoguelikeToolkit.Common.Dice
{
    public readonly struct Dice : IEquatable<Dice>
    {
        public readonly int Die;
        public readonly int Pips;

        public Dice(int die, int pips)
        {
            Pips = pips;
            Die = die;
        }

        #region IEquatable implementaiton

        public bool Equals(Dice other) => Die == other.Die && Pips == other.Pips;

        public override bool Equals(object obj) => obj is Dice other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return (Die * 397) ^ Pips;
            }
        }

        public static bool operator ==(Dice left, Dice right) => left.Equals(right);
        public static bool operator !=(Dice left, Dice right) => !left.Equals(right);

        #endregion
    }
}
