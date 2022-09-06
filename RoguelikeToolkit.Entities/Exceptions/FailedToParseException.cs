using System;

namespace RoguelikeToolkit.Entities.Exceptions;

internal class FailedToParseException : Exception
{
	public FailedToParseException(string failureReason) : base($"Failed to parse a template. Reason: {failureReason}")
	{
	}
	public FailedToParseException(string templateName, string failureReason) : base($"Failed to parse a template (name = {templateName}). Reason: {failureReason}")
	{
	}
}
