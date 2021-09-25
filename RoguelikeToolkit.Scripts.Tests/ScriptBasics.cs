using System;
using DefaultEcs;
using Jint.Runtime;
using RoguelikeToolkit.DiceExpression;
using RoguelikeToolkit.Entities;
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
        public void ComponentScriptCanAffectStructs_with_struct_params()
        {
            var script = new Script("target.x = 5 + foo2.x;");

            var foo = new Foo2();
            var foo2 = new Foo2 { x = 30 };

            script.ExecuteOn(ref foo, ("foo2", foo2));

            Assert.Equal(35, foo.x);
        }

        [Fact]
        public void ComponentScriptCanAffectStructs_with_primitive_params()
        {
            var script = new Script("target.x = 5 + foo;");

            var foo = new Foo2();
            int foobar = 30;

            script.ExecuteOn(ref foo, ("foo", foobar));

            Assert.Equal(35, foo.x);
        }

        [Fact]
        public void EntityScript_should_work()
        {
            var entity = _world.CreateEntity();
            entity.Set(new TestComponent { Dice1 = Dice.Parse("2d+5") });
            
            var c = entity.Get<TestComponent>();

            //sanity
            Assert.Equal(0, c.RollResult);

            var changeScript = new EntityScript(@"
                if(entity.HasComponent(TestComponent)){
                    var component = entity.GetComponent(TestComponent);
                    component.RollResult = component.Dice1.Roll();

                    entity.SetComponent(TestComponent, component);
                }
            ", typeof(TestComponent));

            changeScript.ExecuteOn(ref entity);

            Assert.NotEqual(0, c.RollResult);
        }

        [Fact]
        public void EntityComponentScript_should_work()
        {
            var entity = _world.CreateEntity();
            entity.Set(new TestComponent { Dice1 = Dice.Parse("2d+5") });

            var c = entity.Get<TestComponent>();

            //sanity
            Assert.Equal(0, c.RollResult);

            var changeScript = new EntityComponentScript(@"component.RollResult = component.Dice1.Roll();");

            Assert.True(changeScript.TryExecuteOn<TestComponent>(ref entity));

            Assert.NotEqual(0, c.RollResult);
        }

        [Fact]
        public void EntityComponentScript_without_specific_component_should_not_continue_with_execution()
        {
            var entity = _world.CreateEntity();

            var changeScript = new EntityComponentScript(@"throw 'this is an error!';");

            Assert.False(changeScript.TryExecuteOn<TestComponent>(ref entity)); //this shouldn't throw since there is no such component...
        }

        [Fact]
        public void EntityComponentScript_should_propagate_exceptions()
        {
            var entity = _world.CreateEntity();

            var changeScript = new EntityComponentScript(@"throw 'this is an error!';");
            entity.Set(new TestComponent { Dice1 = Dice.Parse("2d+5") });

            Assert.Throws<JavaScriptException>(() => changeScript.TryExecuteOn<TestComponent>(ref entity));
        }


        [Fact]
        public void EntityComponentScript_with_struct_should_work()
        {
            var entity = _world.CreateEntity();
            entity.Set(new HealthComponent2 { Health = 123 });

            var c = entity.Get<HealthComponent2>();

            //sanity
            Assert.Equal(123, c.Health);

            var changeScript = new EntityComponentScript(@"component.Health = 456;");
            Assert.True(changeScript.TryExecuteOn<HealthComponent2>(ref entity));

            c = entity.Get<HealthComponent2>();
            Assert.Equal(456, c.Health);
        }

        [Fact]
        public void EntityInteractionScript_should_work()
        {
            var caster = _world.CreateEntity();
            caster.Set(new HealthComponent { Health = 50.0 });

            var target = _world.CreateEntity();
            target.Set(new HealthComponent2 { Health = 100.0 });

            var healthStealSpell = new EntityInteractionScript(
                @"        
                    if(source.HasComponent(HealthComponent) === false ||
                       target.HasComponent(HealthComponent2) === false)
                        return;

                    var sourceHealth = source.GetComponent(HealthComponent);
                    var targetHealth = target.GetComponent(HealthComponent2);
                    
                    sourceHealth.Health += 50;
                    targetHealth.Health -= 50;

                    source.SetComponent(HealthComponent, sourceHealth);
                    target.SetComponent(HealthComponent2, targetHealth);
                ", typeof(HealthComponent), typeof(HealthComponent2));

            healthStealSpell.ExecuteOn(ref caster, ref target);

            var sourceHealth = caster.Get<HealthComponent>();
            var targetHealth = target.Get<HealthComponent2>();

            Assert.Equal(100.0, sourceHealth.Health);
            Assert.Equal(50.0, targetHealth.Health);
        }
    }
}
