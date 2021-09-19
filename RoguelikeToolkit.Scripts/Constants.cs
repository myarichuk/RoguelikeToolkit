using System.Collections.Generic;
using System.Reflection;
using DefaultEcs;

namespace RoguelikeToolkit.Scripts
{
    internal static class Constants
    {
        internal static readonly Assembly[] AssembliesToReference =
           {
                Assembly.GetCallingAssembly(),
                Assembly.GetEntryAssembly(),
                typeof(Entity).Assembly,
                typeof(IEnumerable<>).Assembly
           };
    }
}