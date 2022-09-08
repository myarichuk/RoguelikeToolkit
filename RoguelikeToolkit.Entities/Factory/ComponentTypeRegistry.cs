using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Esprima.Ast;
using Fasterflect;
using RoguelikeToolkit.Entities.Components;
using RoguelikeToolkit.Entities.Exceptions;

// ReSharper disable UncatchableException

namespace RoguelikeToolkit.Entities.Factory
{
	//a class that
	//1) scans all referenced assemblies for component types
	//2) translates type name to concrete .Net type
	internal class ComponentTypeRegistry
	{
		private readonly ConcurrentDictionary<string, Type> _typeRegistry = new(StringComparer.InvariantCultureIgnoreCase);

		/// <exception cref="InvalidOperationException">Failed to load assemblies present in the process.</exception>
		public ComponentTypeRegistry()
		{
			try
			{
				var nonFrameworkAssemblies =
					from assembly in AppDomain.CurrentDomain.GetAssemblies()
					where !IsFrameworkAssembly(assembly)
					select assembly;

				var componentTypes =
					from type in nonFrameworkAssemblies.SelectMany(assembly => assembly.GetTypes())
					where type.HasAttribute<ComponentAttribute>() || type.IsValueComponentType()
					select type;

				foreach (var type in componentTypes)
				{
					var componentTypeName = type.Name;
					if (type.HasAttribute<ComponentAttribute>())
						componentTypeName = type.Attribute<ComponentAttribute>().Name ?? type.Name;

					if (!_typeRegistry.TryAdd(componentTypeName, type))
						ThrowConflictingComponentType(type);
				}
			}
			catch (AppDomainUnloadedException ex)
			{
				throw new InvalidOperationException("Failed to load assemblies present in the process. This is not supposed to happen and is likely a bug.", ex);
			}
			catch (OverflowException ex)
			{
				throw new InvalidOperationException(
					"Failed to load component types. Are there too many types marked with 'Component' attribute?", ex);
			}
			catch (ReflectionTypeLoadException ex)
			{
				throw new InvalidOperationException("Failed to load component types. This error is not supposed to happen and is likely due to some unforeseen issue.", ex);
			}

			static bool IsFrameworkAssembly(Assembly assembly) =>
				(assembly.FullName?.Contains("Microsoft.") ?? false) ||
				(assembly.FullName?.Contains("System.") ?? false);
		}

		public bool TryGetComponentType(string typeName, out Type type) =>
			_typeRegistry.TryGetValue(typeName, out type);

		private void ThrowConflictingComponentType(Type conflictingType) =>
			throw new ComponentTypeConflictException(conflictingType.Name, _typeRegistry[conflictingType.Name].FullName);
	}
}
