using YamlDotNet.Serialization;
using System.IO;
using System.Runtime.CompilerServices;

namespace RoguelikeToolkit.Entities.Repository
{

	internal class EntityTemplateLoader
	{
		private readonly IDeserializer _deserializer = new DeserializerBuilder()
						.IgnoreUnmatchedProperties()
						.IgnoreFields()
						.WithAttemptingUnquotedStringTypeDeserialization()
						.Build();

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
		public EntityTemplate LoadFrom(StreamReader sr) => _deserializer.Deserialize<EntityTemplate>(sr) ?? new EntityTemplate();
	}
}
