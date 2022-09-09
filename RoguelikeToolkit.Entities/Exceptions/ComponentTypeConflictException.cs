using System;

namespace RoguelikeToolkit.Entities.Exceptions;

internal class ComponentTypeConflictException : Exception
{
	public ComponentTypeConflictException(string conflictingTypeName, string existingFullTypeName) : base($"Failed to add component type, component with name '{conflictingTypeName}' already exist. (fully qualified type of the other, conflicting assembly is {existingFullTypeName})")
	{
	}
}
