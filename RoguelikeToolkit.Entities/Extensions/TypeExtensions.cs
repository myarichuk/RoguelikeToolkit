using System;
using System.Linq;

namespace RoguelikeToolkit.Entities
{
	internal static class TypeExtensions
	{
		public static bool IsValueComponentType(this Type type) =>
			type.GetInterfaces().Any(i => i.FullName?.Contains("IValueComponent`1") ?? false);
	}
}
