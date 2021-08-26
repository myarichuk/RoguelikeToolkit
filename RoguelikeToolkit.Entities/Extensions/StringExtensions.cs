using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Utf8Json;

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

        public static bool TryDeserialize(this string json, out IDictionary<string, object> data)
        {
            data = null;
            try
            {
                data = (IDictionary<string, object>)JsonSerializer.Deserialize<dynamic>(json);
            }
            catch (JsonParsingException)
            {
                return false;
            }

            return true;
        }

    }
}
