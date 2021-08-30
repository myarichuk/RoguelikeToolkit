using RoguelikeToolkit.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

// credit: taken from https://github.com/Doraku/DefaultEcs/blob/master/source/DefaultEcs.Extension/Children/EntityExtension.cs
namespace DefaultEcs
{
    public static class EntityExtension
    {
        private readonly struct Children
        {
            public readonly HashSet<Entity> Value;

            public Children(HashSet<Entity> value) => Value = value;
        }

        private static readonly HashSet<World> _worlds = new HashSet<World>();

        private static void OnEntityDisposed(in Entity entity)
        {
            if (entity.Has<Children>())
            {
                var children = entity.Get<Children>().Value;
                entity.Remove<Children>();

                foreach (Entity child in children)
                {
                    if (child.IsAlive)
                        child.Dispose();
                }
            }
        }

        public static void SetAsParentOf(this Entity parent, Entity child)
        {
            if (_worlds.Add(parent.World))
            {
                parent.World.SubscribeEntityDisposed(OnEntityDisposed);
                parent.World.SubscribeWorldDisposed(w => _worlds.Remove(w));
            }

            HashSet<Entity> children;
            if (!parent.Has<Children>())
            {
                children = new HashSet<Entity>();
                parent.Set(new Children(children));
            }
            else
            {
                children = parent.Get<Children>().Value;
            }

            children.Add(child);
        }

        public static IEnumerable<Entity> GetChildrenWithTags(this Entity parent, params string[] tags)
        {
            if (parent.Has<Children>())
            {
                foreach (var child in parent.Get<Children>().Value
                                            .Where(e => e.MetadataContainsTags(tags)))
                {
                    yield return child;
                    foreach (var childOfChild in child.GetChildrenWithTags(tags))
                        yield return childOfChild;
                }
            }
        }

        public static IEnumerable<Entity> GetChildren(this Entity parent)
        {
            if (parent.Has<Children>())
            {
                foreach (var child in parent.Get<Children>().Value)
                {
                    foreach (var childOfChild in child.GetChildren())
                        yield return childOfChild;

                    yield return child;
                }
            }
        }

        public static bool TryGet<T>(this Entity entity, out T component)
        {
            component = default;
            if (entity.Has<T>())
            {
                component = entity.Get<T>();
                return true;
            }

            return false;
        }

        private readonly static HashSet<string> EmptyMetadata = new();

        public static ISet<string> Metadata(this Entity entity)
        {
            if(entity.TryGet<MetadataComponent>(out var metadata))
                return metadata.Value;

            return EmptyMetadata;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool MetadataContainsTags(this Entity entity, params string[] tags) => entity.Metadata().IsSupersetOf(tags);        

        public static void RemoveFromParentsOf(this Entity parent, Entity child)
        {
            if (parent.Has<Children>())
                parent.Get<Children>().Value.Remove(child);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetAsChildOf(this Entity child, Entity parent) => parent.SetAsParentOf(child);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveFromChildrenOf(this Entity child, Entity parent) => parent.RemoveFromParentsOf(child);
    }
}