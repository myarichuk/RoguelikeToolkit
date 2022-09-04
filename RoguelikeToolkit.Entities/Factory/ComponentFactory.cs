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

			foreach (var kvp in objectData)
			{
				//note: since ware deserializing yaml, kvp.Key will always be string
				if (kvp.Key is not string destPropertyName) //precaution
					continue; //TODO: add logging here (this should be a warning!)

				var property = GetDestPropertyFor<TComponent>(destPropertyName);

				if (property == null) //we do not enforce 1:1 structural parity
					continue;

				instance.SetPropertyValue(property.Name, ConvertValueFromSrcToDestType(kvp.Value, property.PropertyType));
			}

			return instance;
		}

		private object ConvertValueFromSrcToDestType(object srcValue, Type destType)
		{
			object convertResult;
			switch (srcValue)
			{
				case Dictionary<object, object> valueAsDictionary:
				{
					var createInstanceMethod = MakeGenericCreateInstance(destType);
					convertResult = createInstanceMethod.Call(this, valueAsDictionary);
					break;
				}
				//primitive or string!
				default:
					convertResult =
						_typeConversionProvider.Convert(srcValue.GetType(), destType, srcValue);
					break;
			}

			return convertResult;
		}

		private PropertyInfo GetDestPropertyFor<TComponent>(string srcPropertyName)
		{
			var properties = _typePropertyCache.GetOrAdd(typeof(TComponent),
				type => type.PropertiesWith(Flags.InstancePublic));

			var property = properties.FirstOrDefault(p => p.Name.Equals(srcPropertyName));
			return property;
		}

		private MethodInfo MakeGenericCreateInstance(Type genericParamType)
		{
			var createInstanceMethod = _createInstanceMethodCache.GetOrAdd(genericParamType, propertyType =>
			{
				if (CreateInstanceMethodNonGeneric == null) //precaution, should never be true
					throw new InvalidOperationException(
						"Failed to find CreateInstance method, this is not supposed to happen and is likely a bug");

				var genericParam = _createInstanceGenericsCache.GetOrAdd(propertyType,
					type => new[] { type });

				return CreateInstanceMethodNonGeneric.MakeGenericMethod(genericParam);
			});
			return createInstanceMethod;
		}
	}
}
