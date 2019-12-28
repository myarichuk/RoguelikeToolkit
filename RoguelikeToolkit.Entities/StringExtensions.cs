using System.Runtime.CompilerServices;

namespace RoguelikeToolkit.Entities
{
    public static class StringExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Trim(this string str, int before, int after = 0)
        {
            #if NETSTANDARD2_1
            return str[before..^after];
            #else
            return str.Substring(before, str.Length - after);
            #endif
        }
    }
}
