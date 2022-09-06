using System.Collections.Generic;
using YamlDotNet.Serialization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Fasterflect;
using deniszykov.TypeConversion;
using Microsoft.Extensions.Options;
using RoguelikeToolkit.Entities.Exceptions;

namespace RoguelikeToolkit.Entities.Repository
{

	internal class EntityTemplateLoader
	{
		private readonly IDeserializer _deserializer = new DeserializerBuilder()
						.IgnoreUnmatchedProperties()
						.IgnoreFields()
						.WithAttemptingUnquotedStringTypeDeserialization()
						.Build();

		private static readonly TypeConversionProvider TypeConversionProvider = new(Options.Create(new TypeConversionProviderOptions
		{
			Options = ConversionOptions.UseDefaultFormatIfNotSpecified
		}));

		private static readonly HashSet<string> EmptyHashSet = new();

		/// <exception cref="FailedToParseException">Failed to parse the template for any reason.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public EntityTemplate LoadFrom(FileInfo file)
		{
			using var fs = file.OpenRead();
			using var sr = new StreamReader(fs);

			return LoadFrom(sr);
		}

		/// <exception cref="FailedToParseException">Failed to parse the template for any reason.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public EntityTemplate LoadFrom(string filePath)
		{
			using var fs = File.Open(filePath, FileMode.Open);
			using var sr = new StreamReader(fs);

			return LoadFrom(sr);
		}

		/// <exception cref="FailedToParseException">Failed to parse the template for any reason.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public EntityTemplate LoadFrom(StreamReader sr)
		{
			var rawTemplate = _deserializer.Deserialize<Dictionary<string, object>>(sr);

			return TryLoadFrom(rawTemplate, out var template, out var failureReason) ? template : throw new FailedToParseException(failureReason);
		}

		// ReSharper disable once CognitiveComplexity
		// ReSharper disable once MethodTooLong
		private bool TryLoadFrom(Dictionary<string, object> rawTemplateData, out EntityTemplate template, out string failureReason)
		{
			template = new EntityTemplate();
			failureReason = null;

			foreach (var kvp in rawTemplateData ?? Enumerable.Empty<KeyValuePair<string, object>>())
			{
				if (EntityTemplate.PropertyNames.TryGetValue(kvp.Key, out var properlyCasedPropertyName))
				{
					if (TryHandlePropertyValue(template, properlyCasedPropertyName, kvp.Value))
						continue;

					failureReason = $"Unrecognized property name {kvp.Key}, this is not supposed to happen and is likely a bug";
					return false;
				}

				//we have a embedded template
				if(kvp.Value is Dictionary<object, object> rawEmbeddedTemplate)
				{
					if (TryHandleEmbeddedTemplate(template, rawEmbeddedTemplate, kvp.Key, ref failureReason))
						continue;

					return false;
				}

				failureReason = $"Unexpected property name '{kvp.Key}'. Check whether the template schema is correct";
				return false;
			}

			//make sure ALL required properties were set
			return true;
		}

		private bool TryHandleEmbeddedTemplate(EntityTemplate template,
			Dictionary<object, object> rawEmbeddedTemplate, string embeddedTemplateName, ref string failureReason)
		{
			if (rawEmbeddedTemplate.TryGetValue("$ref", out var refValue) && refValue is string referencedTemplateFilename)
			{
				var embeddedTemplate = LoadFrom(referencedTemplateFilename);
				embeddedTemplate.Name = referencedTemplateFilename;
				template.EmbeddedTemplates.Add(embeddedTemplate);

				return true;
			}

			if (TryHandleEmbeddedTemplate(template, embeddedTemplateName, rawEmbeddedTemplate, out var templateLoadFailureReason))
				return true;

			failureReason = templateLoadFailureReason;
			return false;
		}

		// ReSharper disable once TooManyArguments
		private bool TryHandleEmbeddedTemplate(EntityTemplate template, string embeddedTemplateName, Dictionary<object, object> rawTemplateData, out string failureReason)
		{
			failureReason = null;
			if (!TryLoadFrom(rawTemplateData.ToDictionary(
				    valuePair => TypeConversionProvider.ConvertToString(valuePair.Key),
				    valuePair => valuePair.Value), out var embeddedTemplate, out var loadFailureReason))
			{
				failureReason = loadFailureReason;
				return false;
			}

			embeddedTemplate.Name = embeddedTemplateName;
			template.EmbeddedTemplates.Add(embeddedTemplate);

			return true;
		}

		private static object ParseTemplateField(string propertyName, object propertyValue)
		{
			switch (propertyName)
			{
				case nameof(EntityTemplate.Tags):
				case nameof(EntityTemplate.Inherits):
					return propertyValue is List<object> inheritsObjects
						? new HashSet<string>(inheritsObjects.Cast<string>())
						: EmptyHashSet;
				case nameof(EntityTemplate.Components):
					return propertyValue is not Dictionary<object, object> components
						? null
						: components.ToDictionary(
							kvp => TypeConversionProvider.ConvertToString(kvp.Key),
							kvp => kvp.Value);

				default:
					return null;
			}
		}

		private static bool TryHandlePropertyValue(EntityTemplate template, string propertyName, object propertyValue)
		{
			var templateFieldValue = ParseTemplateField(propertyName, propertyValue);
			if (templateFieldValue == null)
				return false;

			template.TrySetPropertyValue(propertyName, templateFieldValue);
			return true;
		}
	}
}
