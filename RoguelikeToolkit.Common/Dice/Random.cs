using System.Runtime.CompilerServices;
using System.Threading;
using Troschuetz.Random;
using Troschuetz.Random.Generators;

namespace RoguelikeToolkit.Common.Dice
{
    public static class Random
    {
        private static ThreadLocal<IGenerator> _generator = new ThreadLocal<IGenerator>(() =>  new NR3Generator());

        public static IGenerator Generator
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _generator.Value;
        }
    }
}
