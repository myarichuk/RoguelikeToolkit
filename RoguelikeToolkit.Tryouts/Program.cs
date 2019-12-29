﻿using RoguelikeToolkit.Entities;

namespace RoguelikeToolkit.Tryouts
{
    class Program
    {
        static void Main(string[] args)
        {
            var templateCollection = new EntityTemplateCollection("Templates");
            var entityFactory = new EntityFactory(templateCollection);
        }
    }
}
