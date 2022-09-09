using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

		public IReadOnlyDictionary<string, object> Components => _components;

		public IReadOnlySet<string> Inherits => _inherits;

		public IReadOnlySet<string> Tags => _tags;

		[YamlIgnore]
		public HashSet<EntityTemplate> EmbeddedTemplates => _embeddedTemplates;

		internal static readonly HashSet<string> PropertyNames =
			new(typeof(EntityTemplate).Properties(Flags.InstancePublic)
											   .Select(p => p.Name)
											   .Where(propertyName => propertyName != nameof(EmbeddedTemplates)), StringComparer.InvariantCultureIgnoreCase);

		private readonly Dictionary<string, object> _components = new(StringComparer.InvariantCultureIgnoreCase);
		private readonly HashSet<string> _inherits = new(StringComparer.InvariantCultureIgnoreCase);
		private readonly HashSet<string> _tags = new(StringComparer.InvariantCultureIgnoreCase);
		private readonly HashSet<EntityTemplate> _embeddedTemplates = new(EqualityComparer);

		public EntityTemplate() { }

		//note: copy constructor needed for "shallow copy" of records
		/// <exception cref="ArgumentNullException"><paramref name="other"/> is <see langword="null"/></exception>
		protected EntityTemplate(EntityTemplate other)
		{
			if (other == null) //just in case
				throw new ArgumentNullException(nameof(other));

			_components = new Dictionary<string, object>(other.Components, StringComparer.InvariantCultureIgnoreCase);
			_inherits = new HashSet<string>(other.Inherits, StringComparer.InvariantCultureIgnoreCase);
			_tags = new HashSet<string>(other.Tags, StringComparer.InvariantCultureIgnoreCase);
			_embeddedTemplates = new HashSet<EntityTemplate>(other.EmbeddedTemplates);
		}

		internal void MergeWith(EntityTemplate other)
		{
			MergeComponents(other.Components);
			MergeInherits(other.Inherits);
			MergeTags(other.Tags);
			MergeEmbeddedTemplates(other.EmbeddedTemplates);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal void MergeComponents(IReadOnlyDictionary<string, object> otherComponents) =>
			_components.MergeWith(otherComponents);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal void MergeInherits(IReadOnlySet<string> otherInherits) =>
			_inherits.UnionWith(otherInherits);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal void MergeTags(IReadOnlySet<string> otherTags) =>
			_tags.UnionWith(otherTags);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal void MergeEmbeddedTemplates(IReadOnlySet<EntityTemplate> otherEmbeddedTemplates) =>
			_embeddedTemplates.UnionWith(otherEmbeddedTemplates);

	}
}
