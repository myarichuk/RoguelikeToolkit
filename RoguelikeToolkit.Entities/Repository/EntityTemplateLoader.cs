using System.Collections.Generic;
using YamlDotNet.Serialization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Fasterflect;
using deniszykov.TypeConversion;
using Microsoft.Extensions.Options;

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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public EntityTemplate LoadFrom(FileInfo file)
		{
			using var fs = file.OpenRead();
			using var sr = new StreamReader(fs);

			return LoadFrom(sr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public EntityTemplate LoadFrom(string filePath)
		{
			using var fs = File.Open(filePath, FileMode.Open);
			using var sr = new StreamReader(fs);

			return LoadFrom(sr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public EntityTemplate LoadFrom(StreamReader sr)
		{
			var rawTemplate = _deserializer.Deserialize<Dictionary<string, object>>(sr);

			return TryLoadFrom(rawTemplate, out var template) ? template : null;
		}

		private static bool TryLoadFrom(Dictionary<string, object> rawTemplateData, out EntityTemplate template)
		{
			template = new EntityTemplate();


			foreach (var kvp in rawTemplateData ?? Enumerable.Empty<KeyValuePair<string, object>>())
			{
				if (EntityTemplate.PropertyNames.TryGetValue(kvp.Key, out var properlyCasedPropertyName))
				{
					if (!TryHandlePropertyValue(template, properlyCasedPropertyName, kvp.Value))
						return false;
				}
				//we have a embedded template
				else if(kvp.Value is Dictionary<object, object> rawEmbeddedTemplate)
				{
					HandleEmbeddedTemplate(template, kvp.Key, rawEmbeddedTemplate.ToDictionary(valuePair => TypeConversionProvider.ConvertToString(valuePair.Key), valuePair => valuePair.Value));
				}
				else
				{
					return false;
				}
			}

			//make sure ALL required properties were set
			return true;
		}

		private static void HandleEmbeddedTemplate(EntityTemplate template, string embeddedTemplateName, Dictionary<string, object> rawTemplateData)
		{
			if (!TryLoadFrom(rawTemplateData, out var embeddedTemplate))
				return;

			embeddedTemplate.Name = embeddedTemplateName;
			template.EmbeddedTemplates.Add(embeddedTemplate);
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
