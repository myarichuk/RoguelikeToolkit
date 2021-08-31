using System;
using System.Collections.Generic;
using RoguelikeToolkit.DiceExpression;

namespace RoguelikeToolkit.Tryouts
{
    public enum ActionList
    {
        Left,
        Right
    }

    internal class Program
    {

        public static Dictionary<ConsoleKey, ActionList> KeyMappings = new Dictionary<ConsoleKey, ActionList>
        {
            { ConsoleKey.LeftArrow, ActionList.Left },
            { ConsoleKey.RightArrow, ActionList.Right }
        };

        public class FooBar
        {
            public string Name { get; set; }
            public EmbeddedFoobar Embedded { get; set; }
            public int Number { get; set; }
            public bool Flag { get; set; }
        }

        public class EmbeddedFoobar
        {
            public int Number { get; set; }
        }

        private static void Main(string[] args)
        {
            Console.WriteLine(Dice.Parse("1", true).Roll());
            //using var world = new World();
            //var foobarAsString = @"{ ""Name"":""John Dow"", ""Flag"":true, ""Number"":123, ""Embedded"" : { ""Number"":456 } }";
            //var foobarTemplate = ComponentTemplate.ParseFromString(foobarAsString);

            //var newInstance = foobarTemplate.CreateInstance(typeof(FooBar));

            //while (true)
            //{
            //    var dice = Dice.Parse(Console.ReadLine());
            //    var value = dice.Roll();
            //    Console.WriteLine(value);
            //}
            //var templateCollection = new EntityTemplateCollection(".");
            //var entityFactory = new EntityFactory(new World(), templateCollection);

            //var success = entityFactory.TryCreateEntity("actor2", out var actorEntity);
            //Console.WriteLine(success);
            //var foobarComponent = actorEntity.Get<FoobarComponent>();
            //Console.WriteLine(foobarComponent.Dice1.Roll());
            //Console.WriteLine(foobarComponent.Dice2.Roll());

            //actorEntity.Set(new FoobarComponent { Dice1 = Dice.Parse("2d+5") });
            //var c = actorEntity.Get<FoobarComponent>();

            //var changeScript = new EntityComponentScript(@"component.RollResult = component.Dice1.Roll();");
            //await changeScript.RunAsyncOn<FoobarComponent>(actorEntity);

            //Console.WriteLine();
            //Console.WriteLine(c.RollResult);

            //await changeScript.RunAsyncOn<FoobarComponent>(actorEntity);
            //Console.WriteLine(c.RollResult);
        }
    }
}
