using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using deniszykov.TypeConversion;
using Fasterflect;
using Microsoft.Extensions.Options;

namespace RoguelikeToolkit.Entities.Factory
{
	internal class ComponentFactory
	{
		private readonly ConcurrentDictionary<Type, IList<PropertyInfo>> _typePropertyCache = new();
		private readonly ConcurrentDictionary<Type, Type[]> _createInstanceGenericsCache = new();
		private readonly ConcurrentDictionary<Type, MethodInfo> _createInstanceMethodCache = new();

		private readonly TypeConversionProvider _typeConversionProvider = new(Options.Create(new TypeConversionProviderOptions
		{
			Options = ConversionOptions.UseDefaultFormatIfNotSpecified
		}));

		private static readonly MethodInfo CreateInstanceMethodNonGeneric =
			typeof(ComponentFactory).Methods()
				.FirstOrDefault(m => m.Name == nameof(CreateInstance));

		public TComponent CreateInstance<TComponent>(Dictionary<object, object> objectData)
		{
			Debug.Assert(objectData != null, nameof(objectData) + " != null");

			var componentType = typeof(TComponent);
			var instance = (TComponent)RuntimeHelpers.GetUninitializedObject(componentType);
			var properties = _typePropertyCache.GetOrAdd(componentType,
				type => type.PropertiesWith(Flags.InstancePublic));

			foreach (var kvp in objectData)
			{
				//note: since we are deserializing yaml, kvp.Key will always be string
				var property = properties.FirstOrDefault(p =>
					p.Name.Equals(kvp.Key as string ??
					              throw new InvalidOperationException(
						              $"Unexpected template key type, expected string but found " +
						              $"{kvp.Key.GetType().AssemblyQualifiedName}"), StringComparison.InvariantCultureIgnoreCase));

				if (property == null) //we do not enforce 1:1 structural parity
					continue;

				object convertResult;
				if (kvp.Value is Dictionary<object, object> valueAsDictionary)
				{
					var createInstanceMethod = MakeGenericCreateInstance(property);
					convertResult = createInstanceMethod.Call(this, valueAsDictionary);
				}
				else //primitive or string!
				{
					convertResult =
						_typeConversionProvider.Convert(kvp.Value.GetType(), property.PropertyType, kvp.Value);
				}
				
				instance.SetPropertyValue(property.Name, convertResult);
			}

			return instance;
		}

		private MethodInfo MakeGenericCreateInstance(PropertyInfo property)
		{
			var createInstanceMethod = _createInstanceMethodCache.GetOrAdd(property.PropertyType, propertyType =>
			{
				if (CreateInstanceMethodNonGeneric == null) //precaution, should never be true
					throw new InvalidOperationException(
						"Failed to find CreateInstance method, this is not supposed to happen and is likely a bug");

				var genericParam = _createInstanceGenericsCache.GetOrAdd(property.PropertyType,
					type => new[] { propertyType });

				return CreateInstanceMethodNonGeneric.MakeGenericMethod(genericParam);
			});
			return createInstanceMethod;
		}
	}
}
