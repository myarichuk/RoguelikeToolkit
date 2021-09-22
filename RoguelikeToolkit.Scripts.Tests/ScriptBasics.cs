using System;
using System.Reflection;
using System.Threading.Tasks;
using DefaultEcs;
using Jint.Runtime;
using RoguelikeToolkit.DiceExpression;
using Xunit;

namespace RoguelikeToolkit.Scripts.Tests
{
    public class ScriptBasics
    {
        private readonly World _world = new World();

        public class Foo
        {
            public int x;
        }

        public struct Foo2
        {
            public int x;
        }

        [Fact]
        public void ComponentScriptCanAffectClasses()
        {
            var script = new Script("target.x = 5;");

            var foo = new Foo();

            script.ExecuteOn(ref foo);

            Assert.Equal(5, foo.x);
        }

        [Fact]
        public void ComponentScriptCanAffectStructs()
        {
            var script = new Script("target.x = 5;");

            var foo = new Foo2();

            script.ExecuteOn(ref foo);

            Assert.Equal(5, foo.x);
        }

        [Fact]
        public void EntityScript_should_work()
        {
            var entity = _world.CreateEntity();
            entity.Set(new TestComponent { Dice1 = Dice.Parse("2d+5") });

            var c = entity.Get<TestComponent>();

            //sanity
            Assert.Equal(0, c.RollResult);

            var changeScript = new EntityScript(@"component.RollResult = component.Dice1.Roll();");

            changeScript.ExecuteOn<TestComponent>(entity);

            Assert.NotEqual(0, c.RollResult);
        }

        [Fact]
        public void EntityScript_without_specific_component_should_not_continue_with_execution()
        {
            var entity = _world.CreateEntity();

            var changeScript = new EntityScript(@"throw 'this is an error!';");

            changeScript.ExecuteOn<TestComponent>(entity); //this shouldn't throw since there is no such component...
        }

        [Fact]
        public void EntityScript_should_propagate_exceptions()
        {
            var entity = _world.CreateEntity();

            var changeScript = new EntityScript(@"throw 'this is an error!';");
            entity.Set(new TestComponent { Dice1 = Dice.Parse("2d+5") });

            Assert.Throws<JavaScriptException>(() => changeScript.ExecuteOn<TestComponent>(entity));
        }


        [Fact]
        public void EntityComponentScript_with_struct_should_work()
        {
            var entity = _world.CreateEntity();
            entity.Set(new HealthComponent2 { Health = 123 });

            var c = entity.Get<HealthComponent2>();

            //sanity
            Assert.Equal(123, c.Health);

            var changeScript = new EntityScript(@"component.Health = 456;");
            changeScript.ExecuteOn<HealthComponent2>(entity);

            c = entity.Get<HealthComponent2>();
            Assert.Equal(456, c.Health);
        }

        //[Fact]
        //public async Task EntityInteractionScript_should_work()
        //{
        //    var caster = _world.CreateEntity();
        //    caster.Set(new HealthComponent(50.0));

        //    var target = _world.CreateEntity();
        //    target.Set(new HealthComponent(100.0));

        //    var healthStealSpell = new EntityInteractionScript(
        //        @"
        //            var sourceHealth = source.Get<HealthComponent>();
        //            var targetHealth = target.Get<HealthComponent>();

        //            sourceHealth.Health += 50;
        //            targetHealth.Health -= 50;
        //        ", Assembly.GetExecutingAssembly());

        //    await healthStealSpell.RunAsyncOn(caster, target);

        //    var sourceHealth = caster.Get<HealthComponent>();
        //    var targetHealth = target.Get<HealthComponent>();

        //    Assert.Equal(100.0, sourceHealth.Health);
        //    Assert.Equal(50.0, targetHealth.Health);
        //}
    }
}
