using System;
using System.Collections.Generic;

namespace RoguelikeToolkit.Entities
{
	public record EntityTemplate
	{
		public string Name { get; set; }

		public Dictionary<string, object> Components { get; set; } = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);

		public HashSet<string> Inherits { get; set; } = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

		public HashSet<string> Tags { get; set; } = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

		public EntityTemplate() { }

		//note: copy constructor needed for "shallow copy" of records
		protected EntityTemplate(EntityTemplate other)
		{
			Components = new Dictionary<string, object>(other.Components);
			Inherits = new HashSet<string>(other.Inherits);
			Tags = new HashSet<string>(other.Tags);
		}
	}
}
