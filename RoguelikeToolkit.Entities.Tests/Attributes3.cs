namespace RoguelikeToolkit.Entities.Tests
{
    public class Foo
    {
        public string Str;
        public double Num;
    }

    public struct Foo2
    {
        public string Str;
        public double Num;
    }

    public class Attributes3Component
    {
        public int Strength;
        public int Agility;
        public int Wisdom;
        public Foo Bar;
    }

    public struct Attributes4Component
    {
        public int Strength;
        public int Agility;
        public int Wisdom;
        public Foo Bar;
    }

    public struct Attributes5Component
    {
        public int Strength;
        public int Agility;
        public int Wisdom;
        public Foo2 Bar;
    }
}
