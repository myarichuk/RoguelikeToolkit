using System;
using System.Collections.Generic;
using System.Linq;
using Fasterflect;
using YamlDotNet.Serialization;

namespace RoguelikeToolkit.Entities
{
	public record EntityTemplate
	{
		public string Name { get; set; }

		public IReadOnlyDictionary<string, object> Components { get; set; } = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);

		public IReadOnlySet<string> Inherits { get; set; } = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

		public IReadOnlySet<string> Tags { get; set; } = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

		[YamlIgnore]
		public HashSet<EntityTemplate> EmbeddedTemplates { get; set; } = new();

		internal static readonly HashSet<string> PropertyNames =
			new(typeof(EntityTemplate).Properties(Flags.InstancePublic)
											   .Select(p => p.Name)
											   .Where(propertyName => propertyName != nameof(EmbeddedTemplates)), StringComparer.InvariantCultureIgnoreCase);

		public EntityTemplate() { }

		//note: copy constructor needed for "shallow copy" of records
		/// <exception cref="ArgumentNullException"><paramref name="other"/> is <see langword="null"/></exception>
		protected EntityTemplate(EntityTemplate other)
		{
			if (other == null) //just in case
				throw new ArgumentNullException(nameof(other));

			Components = new Dictionary<string, object>(other.Components);
			Inherits = new HashSet<string>(other.Inherits, StringComparer.InvariantCultureIgnoreCase);
			Tags = new HashSet<string>(other.Tags, StringComparer.InvariantCultureIgnoreCase);
			EmbeddedTemplates = new HashSet<EntityTemplate>(other.EmbeddedTemplates);
		}
	}
}
