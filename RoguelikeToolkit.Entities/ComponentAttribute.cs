using System;

namespace RoguelikeToolkit.Entities
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class ComponentAttribute : Attribute
	{
		public string Name { get; set; }

		//TODO: implement support for this (SetSameAs() in ECS)
		/// <summary>
		/// if set to true. component's instance will be shared globally
		/// </summary>
		public bool IsGlobal { get; set; }
	}
}
