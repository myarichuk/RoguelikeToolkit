using System;
using System.Threading.Tasks;
using DefaultEcs;
using RoguelikeToolkit.DiceExpression;
using RoguelikeToolkit.Entities;
using RoguelikeToolkit.Scripts;

namespace RoguelikeToolkit.Tryouts
{
    class Program
    {
        static async Task Main(string[] args)
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

            actorEntity.Set(new FoobarComponent { Dice1 = Dice.Parse("2d+5") });
            var c = actorEntity.Get<FoobarComponent>();

            var changeScript = new EntityComponentScript(@"component.RollResult = component.Dice1.Roll();");
            await changeScript.RunAsyncOn<FoobarComponent>(actorEntity);

            Console.WriteLine();
            Console.WriteLine(c.RollResult);

            await changeScript.RunAsyncOn<FoobarComponent>(actorEntity);
            Console.WriteLine(c.RollResult);
        }
    }
}
