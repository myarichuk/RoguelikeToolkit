using System;
using System.Reflection;
using System.Threading.Tasks;
using DefaultEcs;
using RoguelikeToolkit.DiceExpression;
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
                        ", Assembly.GetExecutingAssembly());

            await changeScript.RunAsyncOn(entity);

            Assert.NotEqual(0, c.RollResult);
        }

        [Fact]
        public async Task EntityScript_should_propagate_exceptions()
        {
            var entity = _world.CreateEntity();

            var changeScript = new EntityScript(@"
                            if(entity.Has<TestComponent>())
                            {
                                var c = entity.Get<TestComponent>();
                                c.RollResult = c.Dice1.Roll();
                            }
                            else //no TestComponent in the entity so we make sure that the exception gets thrown
                            { 
                                throw new InvalidOperationException($""{nameof(TestComponent)} not found!"");
                            }
                        ", Assembly.GetExecutingAssembly());

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await changeScript.RunAsyncOn(entity));
        }

        [Fact]
        public async Task EntityComponentScript_should_work()
        {
            var entity = _world.CreateEntity();
            entity.Set(new TestComponent { Dice1 = Dice.Parse("2d+5") });

            var c = entity.Get<TestComponent>();

            //sanity
            Assert.Equal(0, c.RollResult);

            var changeScript = new EntityComponentScript(@"component.RollResult = component.Dice1.Roll();", Assembly.GetExecutingAssembly());
            await changeScript.RunAsyncOn<TestComponent>(entity);

            Assert.NotEqual(0, c.RollResult);
        }

        [Fact]
        public async Task EntityComponentScript_with_struct_should_work()
        {
            var entity = _world.CreateEntity();
            entity.Set(new HealthComponent2 { Health = 123 });

            var c = entity.Get<HealthComponent2>();

            //sanity
            Assert.Equal(123, c.Health);

            var changeScript = new EntityComponentScript(@"component.Health = 456;", Assembly.GetExecutingAssembly());
            await changeScript.RunAsyncOn<HealthComponent2>(entity);

            c = entity.Get<HealthComponent2>();
            Assert.Equal(456, c.Health);
        }

        [Fact]
        public async Task EntityInteractionScript_should_work()
        {
            var caster = _world.CreateEntity();
            caster.Set(new HealthComponent(50.0));

            var target = _world.CreateEntity();
            target.Set(new HealthComponent(100.0));

            var healthStealSpell = new EntityInteractionScript(
                @"
                    var sourceHealth = source.Get<HealthComponent>();
                    var targetHealth = target.Get<HealthComponent>();

                    sourceHealth.Health += 50;
                    targetHealth.Health -= 50;
                ", Assembly.GetExecutingAssembly());

            await healthStealSpell.RunAsyncOn(caster, target);

            var sourceHealth = caster.Get<HealthComponent>();
            var targetHealth = target.Get<HealthComponent>();

            Assert.Equal(100.0, sourceHealth.Health);
            Assert.Equal(50.0, targetHealth.Health);
        }
    }
}
