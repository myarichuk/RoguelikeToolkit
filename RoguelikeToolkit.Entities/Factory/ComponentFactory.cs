using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using deniszykov.TypeConversion;
using Fasterflect;
using Microsoft.Extensions.Options;
using RoguelikeToolkit.DiceExpression;
using RoguelikeToolkit.Entities.Components;
using RoguelikeToolkit.Scripts;

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

			_typeConversionProvider.RegisterConversion<string, EntityScript>(
				(src, _, __) =>
					new EntityScript(src),
				ConversionQuality.Custom);

			_typeConversionProvider.RegisterConversion<string, EntityComponentScript>(
				(src, _, __) =>
					new EntityComponentScript(src),
				ConversionQuality.Custom);

			_typeConversionProvider.RegisterConversion<string, EntityInteractionScript>(
				(src, _, __) =>
					new EntityInteractionScript(src),
				ConversionQuality.Custom);

			_typeConversionProvider.RegisterConversion<string, Script>(
				(src, _, __) =>
					new Script(src),
				ConversionQuality.Custom);

		}

		/// <summary>
		/// Try and create an instance of specified type from the data provided by the dictionary.
		/// Needed for initializing components deserialized from templates.
		/// </summary>
		/// <param name="componentType">Component type to create</param>
		/// <param name="objectData">Property data, typically received from YamlDotNet deserialization</param>
		/// <param name="instance">resulting instance of the component</param>
		/// <returns>true if instance creation succeeded, false otherwise</returns>
		/// <remarks>This overload is intended for value-type components</remarks>
		/// <exception cref="ArgumentNullException"><paramref name="objectData"/> is <see langword="null"/></exception>
		/// <exception cref="ArgumentException">The parameter componentType doesn't implement <see cref="IValueComponent{TValue}"/>.</exception>
		public bool TryCreateInstance(Type componentType, object objectData, out object instance)
		{
			if (!componentType.IsValueComponentType())
				throw new ArgumentException($"The type doesn't implement IValueComponent<T>", nameof(componentType));

			if (objectData == null)
				throw new ArgumentNullException(nameof(objectData));

			instance = default;
			var instanceAsObject = CreateEmptyInstance(componentType);

			var valueComponentType = componentType.GetInterfaces()
				.First(i => i.FullName.Contains(nameof(IValueComponent<object>)));

			instanceAsObject.SetPropertyValue(nameof(IValueComponent<object>.Value), _typeConversionProvider.Convert(objectData.GetType(), valueComponentType.GenericTypeArguments[0], objectData));

			instance = instanceAsObject.UnwrapIfWrapped();
			return true;
		}

		/// <summary>
		/// Try and create an instance of specified type from the data provided by the dictionary.
		/// Needed for initializing components deserialized from templates.
		/// </summary>
		/// <param name="componentType">Component type to create</param>
		/// <param name="objectData">Property data, typically received from YamlDotNet deserialization</param>
		/// <param name="instance">resulting instance of the component</param>
		/// <returns>true if instance creation succeeded, false otherwise</returns>
		/// <remarks>This overload is intended for object components with properties</remarks>
		/// <exception cref="ArgumentNullException"><paramref name="objectData"/> is <see langword="null"/></exception>
		/// <exception cref="ArgumentException">The type implements <see cref="IValueComponent{TValue}"/>, use the other overload for correct functionality.</exception>
		public bool TryCreateInstance(Type componentType, IReadOnlyDictionary<object, object> objectData, out object instance)
		{
			if (objectData == null)
				throw new ArgumentNullException(nameof(objectData));

			if (componentType.IsValueComponentType())
				throw new ArgumentException($"The type implements IValueComponent<T>, use the other overload for correct functionality", nameof(componentType));

			instance = default;
			var instanceAsObject = CreateEmptyInstance(componentType);

			foreach (var kvp in objectData)
			{
				if (!TryGetDestPropertyFor(componentType, kvp.Key, out var property))
					continue;

				//note: ConvertValueFromSrcToDestType() can call recursively to this method (TryCreateInstance)
				if (!instanceAsObject.TrySetPropertyValue(property.Name,
					    ConvertValueFromSrcToDestType(kvp.Value, property.PropertyType)))
				{
					//TODO: add logging for the failure
					//TODO: consider adding a switch parameter (with default = false) to throw in case of failure
					return false;
				}
			}

			instance = instanceAsObject.UnwrapIfWrapped();
			return true;
		}

		/// <summary>
		/// Try and create an instance of specified type from the data provided by the dictionary.
		/// Needed for initializing components deserialized from templates.
		/// </summary>
		/// <typeparam name="TComponent">Type of the object to populate with the data</typeparam>
		/// <param name="objectData">Property data, typically received from YamlDotNet deserialization</param>
		/// <param name="instance">resulting instance of the component</param>
		/// <returns>true if instance creation succeeded, false otherwise</returns>
		/// <exception cref="ArgumentNullException"><paramref name="objectData"/> is <see langword="null"/></exception>
		public bool TryCreateInstance<TComponent>(IReadOnlyDictionary<object, object> objectData, out TComponent instance)
		{
			var success = TryCreateInstance(typeof(TComponent), objectData, out var instanceAsObject);
			instance = (TComponent)instanceAsObject;
			return success;
		}

		private static object CreateEmptyInstance(Type type) =>
			RuntimeHelpers.GetUninitializedObject(type).WrapIfValueType();

		private object ConvertValueFromSrcToDestType(object srcValue, Type destType)
		{
			object convertResult;
			switch (srcValue)
			{
				case Dictionary<object, object> valueAsDictionary:
				{
					TryCreateInstance(destType, valueAsDictionary, out convertResult);
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

		private bool TryGetDestPropertyFor(Type componentType, object destPropertyKey, out PropertyInfo property)
		{
			property = null;

			//note: since ware deserializing yaml, kvp.Key will always be string, this is precaution
			return destPropertyKey is string destPropertyName && 
			       TryGetDestPropertyFor(componentType, destPropertyName, out property);
		}

		private bool TryGetDestPropertyFor(Type componentType, string srcPropertyName, out PropertyInfo property)
		{
			var properties = _typePropertyCache.GetOrAdd(componentType,
				type => type.PropertiesWith(Flags.InstancePublic));

			property = properties.FirstOrDefault(p => p.Name.Equals(srcPropertyName, StringComparison.InvariantCultureIgnoreCase));
			return property != null;
		}
	}
}
