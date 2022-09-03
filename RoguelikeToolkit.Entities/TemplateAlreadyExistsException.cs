using System;

namespace RoguelikeToolkit.Entities
{
	public class TemplateAlreadyExistsException : Exception
	{
		public TemplateAlreadyExistsException(string templateName) : base($"Template (name = {templateName}) already exists")
		{
		}
	}
}
