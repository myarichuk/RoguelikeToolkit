using System;
using DefaultEcs;
using RoguelikeToolkit.DiceExpression;
using RoguelikeToolkit.Entities;

namespace RoguelikeToolkit.Tryouts
{
    class Program
    {
        static void Main(string[] args)
        {
            //while (true)
            //{
            //    var dice = Dice.Parse(Console.ReadLine());
            //    var value = dice.Roll();
            //    Console.WriteLine(value);
            //}
            var templateCollection = new EntityTemplateCollection(".");
            var entityFactory = new EntityFactory(new World(), templateCollection);

            var success = entityFactory.TryCreateEntity("actor2", out var actorEntity);
            Console.WriteLine(success);
            var foobarComponent = actorEntity.Get<FoobarComponent>();
            Console.WriteLine(foobarComponent.Dice1.Roll());
            Console.WriteLine(foobarComponent.Dice2.Roll());
        }
    }
}
