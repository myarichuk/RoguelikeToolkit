﻿using DefaultEcs;
using System.Collections.Generic;
using System.Reflection;

namespace RoguelikeToolkit.Scripts
{
    internal static class Constants
    {
        internal readonly static Assembly[] AssembliesToReference =
           {
                Assembly.GetExecutingAssembly(),
                Assembly.GetCallingAssembly(),
                Assembly.GetEntryAssembly(),
                typeof(Entity).Assembly,
                typeof(IEnumerable<>).Assembly
           };
    }
}