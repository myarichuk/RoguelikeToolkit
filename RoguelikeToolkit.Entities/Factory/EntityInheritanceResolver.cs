using System;

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
			var templateCopy = flatTemplate with { };

			foreach (var inheritedTemplateName in flatTemplate.Inherits)
			{
				if (_tryGetByName(inheritedTemplateName, out var fetchedTemplate))
				{
					var embeddedEffectiveTemplate = GetEffectiveTemplate(fetchedTemplate);
					templateCopy.Components.MergeWith(embeddedEffectiveTemplate.Components); //no key overrides!
					templateCopy.Tags.UnionWith(embeddedEffectiveTemplate.Tags);
					templateCopy.Inherits.UnionWith(embeddedEffectiveTemplate.Inherits);
				}
				else
				{
					throw new InvalidOperationException(
						$"Inherited template name '{inheritedTemplateName}' in  not found. " +
									$"(check template with name = '{flatTemplate.Name}')");
				}
			}

			return templateCopy;
		}
	}
}
