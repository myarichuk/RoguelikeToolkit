using System.IO;

namespace RoguelikeToolkit.Entities
{
	public partial class EntityTemplateFactory
	{
		internal EntityTemplateFactory(DirectoryInfo folder)
		{
		}

		public static EntityTemplateFactory FromFolder(DirectoryInfo folder) =>
			new EntityTemplateFactory(folder);
	}
}
