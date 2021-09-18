using System;
using System.Linq;
using DefaultEcs;
using Xunit;

namespace RoguelikeToolkit.Entities.Tests
{
    public class ChildrenEntityTests : IDisposable
    {
        private readonly World _world = new ();


        [Fact]
        public void Can_iterate_through_children()
        {
            var a = _world.CreateEntity();
            var b = _world.CreateEntity();
            var c = _world.CreateEntity();
            var d = _world.CreateEntity();

            a.SetAsParentOf(b);
            b.SetAsParentOf(c);
            c.SetAsParentOf(d);

            var children = a.GetChildren().ToList();

            Assert.Equal(3, children.Count);
            Assert.Equal(b, children[0]);
            Assert.Equal(c, children[1]);
            Assert.Equal(d, children[2]);
        }

        [Fact]
        public void Should_return_empty_when_no_tags()
        {
            var a = _world.CreateEntity();
            var b = _world.CreateEntity();
            var c = _world.CreateEntity();
            var d = _world.CreateEntity();

            a.SetAsParentOf(b);
            b.SetAsParentOf(c);
            c.SetAsParentOf(d);

            Assert.Empty(a.GetChidrenWithTags("arm"));
        }

        [Fact]
        public void Should_return_entities_with_tags()
        {
            var entityFactory = EntityFactory.Construct()
                .WithTemplatesFrom("Templates")
                .Build();

            Assert.True(entityFactory.TryCreateEntity("actor", out var actorEntity));

            var childrenWithTags = actorEntity.GetChidrenWithTags("arm").ToList();

            Assert.Equal(2, childrenWithTags.Count);
        }

        public void Dispose() => _world.Dispose();
    }
}
