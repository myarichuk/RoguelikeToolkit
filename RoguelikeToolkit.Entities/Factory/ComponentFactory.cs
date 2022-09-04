using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using deniszykov.TypeConversion;
using Fasterflect;
using Microsoft.Extensions.Options;
using RoguelikeToolkit.DiceExpression;

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
				.FirstOrDefault(m => m.Name == nameof(TryCreateInstance));

		public ComponentFactory()
		{
			_typeConversionProvider.RegisterConversion<string, Dice>(
				(src, _, __) =>
					Dice.Parse(src, true),
				ConversionQuality.Custom);
		}

		/// <exception cref="ArgumentNullException"><paramref name="objectData"/> is <see langword="null"/></exception>
		public bool TryCreateInstance<TComponent>(Dictionary<object, object> objectData, out TComponent instance)
		{
			if (objectData == null)
				throw new ArgumentNullException(nameof(objectData));

			instance = default;
			var instanceAsObject = CreateEmptyInstance<TComponent>();

			foreach (var kvp in objectData)
			{
				if (!TryGetDestPropertyFor<TComponent>(kvp.Key, out var property))
					continue;

				if (!instanceAsObject.TrySetPropertyValue(property.Name,
					    ConvertValueFromSrcToDestType(kvp.Value, property.PropertyType)))
				{
					//TODO: add logging for the failure
					return false;
				}
			}

			instance = (TComponent)instanceAsObject.UnwrapIfWrapped();
			return true;
		}

		private static object CreateEmptyInstance<TComponent>() =>
			RuntimeHelpers.GetUninitializedObject(typeof(TComponent)).WrapIfValueType();

		private object ConvertValueFromSrcToDestType(object srcValue, Type destType)
		{
			object convertResult;
			switch (srcValue)
			{
				case Dictionary<object, object> valueAsDictionary:
				{
					var createInstanceMethod = MakeGenericCreateInstance(destType);
					var @params = new object[] { valueAsDictionary, null };
					createInstanceMethod.Call(this, @params);
					convertResult = @params[1];

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

		private bool TryGetDestPropertyFor<TComponent>(object srcPropertyName, out PropertyInfo property)
		{
			property = null;

			//note: since ware deserializing yaml, kvp.Key will always be string, this is precaution
			return srcPropertyName is string destPropertyName && 
			       TryGetDestPropertyFor<TComponent>(destPropertyName, out property);
		}

		private bool TryGetDestPropertyFor<TComponent>(string srcPropertyName, out PropertyInfo property)
		{
			var properties = _typePropertyCache.GetOrAdd(typeof(TComponent),
				type => type.PropertiesWith(Flags.InstancePublic));

			property = properties.FirstOrDefault(p => p.Name.Equals(srcPropertyName, StringComparison.InvariantCultureIgnoreCase));
			return property != null;
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
