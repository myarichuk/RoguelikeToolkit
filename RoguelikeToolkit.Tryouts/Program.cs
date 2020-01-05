using System;
using System.ComponentModel.DataAnnotations;
using DefaultEcs;
using RoguelikeToolkit.Entities;

namespace RoguelikeToolkit.Tryouts
{
    class Program
    {
        static void Main(string[] args)
        {
            
            var templateCollection = new EntityTemplateCollection("Templates");
            var entityFactory = new EntityFactory(new World(), templateCollection);

            var success = entityFactory.TryCreateEntity("actor", out var actorEntity);
        }
    }
}
