using System;
using System.Collections.Generic;

namespace RoguelikeToolkit.Entities
{
	public record EntityTemplate
	{
		public string Id { get; private set; }

		public Dictionary<string, object> Components { get; } = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);

		public HashSet<string> Inherits { get; } = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

		public HashSet<string> Tags { get; } = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

		public Dictionary<string, string> ChildrenById { get; } = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

		public EntityTemplate() { }
	}
}
