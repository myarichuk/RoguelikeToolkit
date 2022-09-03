using System;

namespace RoguelikeToolkit.Entities
{
	public class TemplateAlreadyExistsException : Exception
	{
		public TemplateAlreadyExistsException(string templateName) : base($"Template (name = {templateName}) already exists. Note that 'foo.yaml' and 'foo.json' would be considered as the same template")
		{
		}
	}
}
