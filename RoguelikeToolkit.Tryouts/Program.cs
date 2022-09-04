using System;
using System.Collections.Generic;
using RoguelikeToolkit.DiceExpression;
using Syllabore;

namespace RoguelikeToolkit.Tryouts
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
	        var dice = Dice.Parse("3d6", true);
	        Console.WriteLine(dice.Roll());
	        Console.WriteLine(dice.Roll());

	        //var provider = new DefaultSyllableProvider();
	        //var validator = new NameValidator()
	        //        .DoNotAllowPattern(@"[j|p|q|w]$")             // Invalidate these awkward endings
	        //        .DoNotAllowPattern(@"(\w)\1\1")               // Invalidate any sequence of 3 or more identical letters
	        //        .DoNotAllowPattern(@"([^aeiouAEIOU])\1\1\1"); // Invalidate any sequence of 4 or more consonants

	        //var g = new NameGenerator(provider, validator);

	        //for (int i = 0; i < 10; i++)
	        //{
	        //    Console.WriteLine(g.Next());
	        //}

	        //Console.WriteLine("---");

	        //// You can choose to build name generators programmatically.
	        //g = new NameGenerator()
	        //    .UsingProvider(x => x
	        //        .WithLeadingConsonants("str")
	        //        .WithVowels("ae"))
	        //    .LimitSyllableCount(3);

	        //for (int i = 0; i < 10; i++)
	        //{
	        //    Console.WriteLine(g.Next());
	        //}

	        //Console.WriteLine("---");

	        //g = new NameGenerator().UsingMutator(new VowelMutator());

	        //for (int i = 0; i < 3; i++)
	        //{
	        //    var name = g.NextName();

	        //    Console.WriteLine(name);

	        //    for (int j = 0; j < 4; j++)
	        //    {
	        //        var variation = g.Mutator.Mutate(name);
	        //        Console.WriteLine(variation);

	        //    }
	        //}

	        //Console.WriteLine("---");
	        //g = new NameGenerator()
	        //                    .UsingProvider(p => p
	        //                        .WithVowels("aeoy")
	        //                        .WithLeadingConsonants("vstlr")
	        //                        .WithTrailingConsonants("zrt")
	        //                        .WithVowelSequences("ey", "ay", "oy"))
	        //                    .UsingValidator(v => v
	        //                        .DoNotAllowPattern(
	        //                            @".{12,}",
	        //                            @"(\w)\1\1",             // Prevents any letter from occuring three times in a row
	        //                            @".*([y|Y]).*([y|Y]).*", // Prevents double y
	        //                            @".*([z|Z]).*([z|Z]).*", // Prevents double z
	        //                            @"(zs)",                 // Prevents "zs"
	        //                            @"(y[v|t])"))            // Prevents "yv" and "yt"
	        //                    .LimitMutationChance(0.99)
	        //                    .LimitSyllableCount(2, 4);

	        //ConfigurationFile.Save(g, "city-name-generator.txt");
	        //var g2 = ConfigurationFile.Load("city-name-generator.txt");

	        //for (int i = 0; i < 50; i++)
	        //{
	        //    var name = g.NextName();
	        //    Console.WriteLine(name);
	        //}

        }
    }
}
