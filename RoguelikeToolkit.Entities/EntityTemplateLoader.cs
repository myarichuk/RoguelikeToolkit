using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using System.IO;

namespace RoguelikeToolkit.Entities
{
	internal class EntityTemplateLoader
	{
		private readonly IDeserializer _deserializer = new DeserializerBuilder()
						.WithNamingConvention(CamelCaseNamingConvention.Instance)
						.IgnoreUnmatchedProperties()
						.IgnoreFields()
						.WithAttemptingUnquotedStringTypeDeserialization()
						.Build();

		public EntityTemplate LoadFrom(FileInfo file)
		{
			using var fs = file.OpenRead();
			using var sr = new StreamReader(fs);

			return _deserializer.Deserialize<EntityTemplate>(sr) ?? new EntityTemplate();
		}
	}
}
