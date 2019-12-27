using RoguelikeToolkit.Entities;

namespace RoguelikeToolkit.Tryouts
{
    class Program
    {
        static void Main(string[] args)
        {
            var templateContainer = new EntityTemplateContainer();
            templateContainer.LoadTemplate("object.json");
        }
    }
}
