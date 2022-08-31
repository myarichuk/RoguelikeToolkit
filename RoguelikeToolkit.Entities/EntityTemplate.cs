using System;
using System.Collections.Generic;

namespace RoguelikeToolkit.Entities
{
	public record EntityTemplate
	{
		public string Id { get; private set; }

		public Dictionary<string, object> Components { get; set; } = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);

		public HashSet<string> Inherits { get; set; } = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

		public HashSet<string> Tags { get; set; } = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

		public EntityTemplate() { }
	}
}
