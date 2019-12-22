using System.IO;
using RoguelikeToolkit.Common.EntityTemplates;

namespace RoguelikeToolkit.Tryouts
{
    class Program
    {
        static void Main(string[] args)
        {
            var actorTemplate = EntityTemplate.LoadFromFile(File.OpenRead("actor.json"));
            var templateCollection = new EntityTemplateCollection(".");
        }
    }
}
