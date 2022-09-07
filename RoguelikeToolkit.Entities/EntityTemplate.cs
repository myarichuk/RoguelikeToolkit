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

		#region Equality Comparer
		private sealed class NameEqualityComparer : IEqualityComparer<EntityTemplate>
		{
			public bool Equals(EntityTemplate x, EntityTemplate y)
			{
				if (ReferenceEquals(x, y)) return true;
				if (ReferenceEquals(x, null)) return false;
				if (ReferenceEquals(y, null)) return false;
				if (x.GetType() != y.GetType()) return false;
				return string.Equals(x.Name, y.Name, StringComparison.InvariantCultureIgnoreCase);
			}

			public int GetHashCode(EntityTemplate obj)
			{
				return (obj.Name != null ? StringComparer.InvariantCultureIgnoreCase.GetHashCode(obj.Name) : 0);
			}
		}

		public static IEqualityComparer<EntityTemplate> EqualityComparer { get; } = new NameEqualityComparer();
		#endregion

		public IReadOnlyDictionary<string, object> Components { get; set; } = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);

		public IReadOnlySet<string> Inherits { get; set; } = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

		public IReadOnlySet<string> Tags { get; set; } = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

		[YamlIgnore]
		public HashSet<EntityTemplate> EmbeddedTemplates { get; set; } = new(EqualityComparer);

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

			Components = new Dictionary<string, object>(other.Components, StringComparer.InvariantCultureIgnoreCase);
			Inherits = new HashSet<string>(other.Inherits, StringComparer.InvariantCultureIgnoreCase);
			Tags = new HashSet<string>(other.Tags, StringComparer.InvariantCultureIgnoreCase);
			EmbeddedTemplates = new HashSet<EntityTemplate>(other.EmbeddedTemplates);
		}

		internal void MergeWith(EntityTemplate other)
		{
			var newComponents = new Dictionary<string, object>(other.Components, StringComparer.InvariantCultureIgnoreCase);
			newComponents.MergeWith(Components);
			Components = newComponents;
			
			Inherits = new HashSet<string>(other.Inherits.Union(Inherits), StringComparer.InvariantCultureIgnoreCase);
			Tags = new HashSet<string>(other.Tags.Union(Tags), StringComparer.InvariantCultureIgnoreCase);
			EmbeddedTemplates = new HashSet<EntityTemplate>(other.EmbeddedTemplates.Union(EmbeddedTemplates, EqualityComparer), EqualityComparer);
		}
	}
}
