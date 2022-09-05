using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;

namespace RoguelikeToolkit.Entities.Factory
{
	internal delegate bool TryGetTemplateByName(string templateName, out EntityTemplate template);

	internal class EntityInheritanceResolver
	{
		private readonly TryGetTemplateByName _tryGetByName;

		public EntityInheritanceResolver(TryGetTemplateByName getByIdFunc) =>
			_tryGetByName = getByIdFunc;

		//traverse the components inheritance and merge all the components
		public EntityTemplate GetEffectiveTemplate(EntityTemplate flatTemplate)
		{
			//note: this executes copy constructor (feature of C# records!)
			//note 2: if no copy constructor present, this will create shallow clone (inner properties would be the same)
			Debug.Assert(flatTemplate != null, nameof(flatTemplate) + " != null");

			var templateCopy = flatTemplate with { };

			foreach (var inheritedTemplateName in flatTemplate.Inherits)
			{
				if (_tryGetByName(inheritedTemplateName, out var inheritedTemplate))
				{
					var embeddedEffectiveTemplate = GetEffectiveTemplate(inheritedTemplate);
					MergeInheritedTemplates(embeddedEffectiveTemplate, templateCopy);
				}
				else
				{
					HandleMissingInheritance(flatTemplate, inheritedTemplateName);
				}
			}

			return templateCopy;
		}

		private static void MergeInheritedTemplates(EntityTemplate srcTemplate, EntityTemplate destTemplate)
		{
			destTemplate.Components.MergeWith(srcTemplate.Components); //no key overrides!
			destTemplate.Tags.UnionWith(srcTemplate.Tags ?? Enumerable.Empty<string>());
			destTemplate.Inherits.UnionWith(srcTemplate.Inherits ?? Enumerable.Empty<string>());
		}

		private static void HandleMissingInheritance(EntityTemplate flatTemplate, string inheritedTemplateName)
		{
			throw new InvalidOperationException(
				$"Inherited template name '{inheritedTemplateName}' in  not found. " +
				$"(check template with name = '{flatTemplate.Name}')");
			//TODO: add logging in this case
		}
	}
}
