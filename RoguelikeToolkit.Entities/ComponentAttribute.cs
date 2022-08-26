using System;

namespace RoguelikeToolkit.Entities
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class ComponentAttribute : Attribute
	{
		public string Name { get; set; }
	}
}
