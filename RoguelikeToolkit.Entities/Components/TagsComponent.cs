using System.Collections.Generic;

namespace RoguelikeToolkit.Entities.Components
{
	public record struct TagsComponent : IValueComponent<HashSet<string>>
	{
		public HashSet<string> Value { get; set; }
	}
}
