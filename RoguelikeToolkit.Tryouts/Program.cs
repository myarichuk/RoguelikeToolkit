using System;
using System.IO;
using DefaultEcs;
using RoguelikeToolkit.Common.Entities;
using RoguelikeToolkit.Common.EntityTemplates;

namespace RoguelikeToolkit.Tryouts
{
    class Program
    {
        static void Main(string[] args)
        {
            var templateRepository = new EntityTemplateRepository(".");
            var entityFactory = new EntityFactory(templateRepository);
            var world = new World();

            var actorEntity = world.CreateEntity();

            Console.WriteLine(entityFactory.TryCreate("actor", ref actorEntity));
        }
    }
}
