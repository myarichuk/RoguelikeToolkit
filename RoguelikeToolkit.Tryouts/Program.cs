using System;
using DefaultEcs;
using RoguelikeToolkit.Common.EntityTemplates;

namespace RoguelikeToolkit.Tryouts
{
    class Program
    {
        static void Main(string[] args)
        {
            var actorTemplate = EntityTemplate.LoadFromJson("actor-template.json");
        }
    }
}
