using System;
using System.Diagnostics;

namespace RoguelikeToolkit.Entities.Factory
{
	internal delegate bool TryGetTemplateByName(string templateName, out EntityTemplate template);

	internal class EntityInheritanceResolver
	{
		private readonly TryGetTemplateByName _tryGetByName;

		public EntityInheritanceResolver(TryGetTemplateByName getByIdFunc) =>
			_tryGetByName = getByIdFunc;

		//traverse the components inheritance and merge all the components
		public EntityTemplate GetEffectiveTemplate(EntityTemplate flatTemplate, bool throwOnMissingInheritance = true)
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
					templateCopy.Components.MergeWith(embeddedEffectiveTemplate.Components); //no key overrides!
					templateCopy.Tags.UnionWith(embeddedEffectiveTemplate.Tags);
					templateCopy.Inherits.UnionWith(embeddedEffectiveTemplate.Inherits);
				}
				else
				{
					if (throwOnMissingInheritance)
					{
						throw new InvalidOperationException(
							$"Inherited template name '{inheritedTemplateName}' in  not found. " +
							$"(check template with name = '{flatTemplate.Name}')");
					}
					else
					{
						//TODO: add logging in this case
					}
				}
			}

			return templateCopy;
		}
	}
}
