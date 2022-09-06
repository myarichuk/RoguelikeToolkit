using System;
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


			foreach (var kvp in rawTemplateData)
			{
				if (EntityTemplate.PropertyNames.Contains(kvp.Key))
				{
					if (!TryHandlePropertyValue(template, kvp))
						return false;
				}
				//we have a embedded template
				else if(kvp.Value is Dictionary<string, object> rawEmbeddedTemplate)
				{
					HandleEmbeddedTemplate(template, kvp.Key, rawEmbeddedTemplate);
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
			template.Children.Add(embeddedTemplate);
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

		private static bool TryHandlePropertyValue(EntityTemplate template, KeyValuePair<string, object> kvp)
		{
			var templateFieldValue = ParseTemplateField(kvp.Key, kvp.Value);
			if (templateFieldValue == null)
				return false;

			template.TrySetPropertyValue(kvp.Key, templateFieldValue);
			return true;
		}
	}
}
