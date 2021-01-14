using DefaultEcs;
using RoguelikeToolkit.DiceExpression;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace RoguelikeToolkit.Scripts.Tests
{
    public class ScriptBasics
    {
        private readonly World _world = new World();

        [Fact]
        public async Task EntityScript_should_work()
        {
            var entity = _world.CreateEntity();
            entity.Set(new TestComponent { Dice1 = Dice.Parse("2d+5") });

            var c = entity.Get<TestComponent>();

            //sanity
            Assert.Equal(0, c.RollResult);

            var changeScript = new EntityScript(@"
                            if(entity.Has<TestComponent>())
                            {
                                var c = entity.Get<TestComponent>();
                                c.RollResult = c.Dice1.Roll();
                            }
                        ");

            await changeScript.RunAsyncOn(entity);

            Assert.NotEqual(0, c.RollResult);
        }

        [Fact]
        public async Task EntityComponentScript_should_work()
        {
            var entity = _world.CreateEntity();
            entity.Set(new TestComponent { Dice1 = Dice.Parse("2d+5") });

            var c = entity.Get<TestComponent>();

            //sanity
            Assert.Equal(0, c.RollResult);

            var changeScript = new EntityComponentScript(@"component.RollResult = component.Dice1.Roll();");
            await changeScript.RunAsyncOn<TestComponent>(entity);

            Assert.NotEqual(0, c.RollResult);
        }
    }
}
