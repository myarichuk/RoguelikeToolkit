using System;
using System.Linq;
using System.Reflection;

namespace RoguelikeToolkit.Scripts
{
    internal static class Constants
    {
        internal readonly static Assembly[] AssembliesToReference =
            AppDomain.CurrentDomain.GetAssemblies()
                .Where(asm => !asm.IsDynamic && !string.IsNullOrWhiteSpace(asm.Location))
                .ToArray();

    }
}
