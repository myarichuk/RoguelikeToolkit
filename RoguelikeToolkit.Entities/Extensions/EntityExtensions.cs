using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using DefaultEcs;
using Microsoft.Extensions.ObjectPool;
using RoguelikeToolkit.Entities;
using RoguelikeToolkit.Entities.Components;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System;
using RoguelikeToolkit.Scripts;
using System.Threading;

namespace RoguelikeToolkit
{
    //credit: taken from https://github.com/Doraku/DefaultEcs/blob/master/source/DefaultEcs.Extension/Children/EntityExtension.cs
    //note: license is MIT, so copying this should be ok (https://github.com/Doraku/DefaultEcs/blob/master/LICENSE.md)
    public static class EntityExtension
    {
        private readonly struct Children
        {
            public readonly HashSet<Entity> Value;

            public Children(HashSet<Entity> value)
            {
                Value = value;
            }
        }

        private static readonly HashSet<string> EmptyMetadata = new();
        private static readonly HashSet<World> Worlds = new();
        private static readonly ObjectPool<HashSet<Entity>> VisitedPool = Entities.ObjectPoolProvider.Instance.Create(new Entities.ThreadSafeObjectPoolPolicy<HashSet<Entity>>());

        private static void OnEntityDisposed(in Entity entity)
        {
            if (entity.Has<Children>())
            {
                HashSet<Entity> children = entity.Get<Children>().Value;
                entity.Remove<Children>();

                foreach (Entity child in children)
                {
                    if (child.IsAlive)
                    {
                        child.Dispose();
                    }
                }
            }
        }

        public static Task RunScript<TComponent>(this Entity entity, Func<TComponent, EntityScript> scriptSelector, CancellationToken ct = default)
        {
            if (!entity.TryGet<TComponent>(out var component))
                ThrowNoComponent<TComponent>();

            var script = scriptSelector(component);

            return script.RunAsyncOn(entity, ct);
        }

        public static Task RunScript<TComponent>(this Entity entity, Func<TComponent, EntityComponentScript> scriptSelector, CancellationToken ct = default)
        {
            if (!entity.TryGet<TComponent>(out var component))
                ThrowNoComponent<TComponent>();

            var script = scriptSelector(component);

            return script.RunAsyncOn<TComponent>(entity, ct);
        }

        private static void ThrowNoComponent<TComponent>() => 
            throw new InvalidOperationException($"Failed to run the script on {typeof(TComponent).FullName} because the component is not attached to the entity");

        public static string Id(this Entity entity) =>
            entity.TryGet<IdComponent>(out var id) ? id.Value : null;

        public static ISet<string> Metadata(this Entity entity)
        {
            if (entity.TryGet<MetadataComponent>(out var metadata))
            {
                return metadata.Value;
            }

            return EmptyMetadata;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasTags(this Entity entity, params string[] tags) => entity.Metadata().IsSupersetOf(tags);

        public static IEnumerable<Entity> GetChidrenWithTags(this Entity parent, params string[] tags)
        {
            if (!parent.Has<Children>())
            {
                yield break;
            }

            foreach (var child in parent.GetChildren().Where(ch => ch.HasTags(tags)))
            {
                yield return child;

                foreach (var childOfChild in child.GetChidrenWithTags(tags))
                {
                    yield return childOfChild;
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


        public static IEnumerable<Entity> GetChildren(this Entity parent)
        {
            if (!parent.Has<Children>())
            {
                yield break;
            }

            HashSet<Entity> visited = null;

            try
            {
                visited = VisitedPool.Get();
                var children = parent.Get<Children>().Value;

                foreach (var child in children)
                {
                    if (visited.Contains(child))
                    {
                        continue;
                    }

                    visited.Add(child);
                    yield return child;

                    foreach (var childOfChild in child.GetChildren())
                    {
                        yield return childOfChild;
                    }
                }
            }
            finally
            {
                if (visited != null)
                {
                    visited.Clear();
                    VisitedPool.Return(visited);
                }
            }
        }

        public static void SetAsParentOf(this Entity parent, Entity child)
        {
            if (Worlds.Add(parent.World))
            {
                parent.World.SubscribeEntityDisposed(OnEntityDisposed);
                parent.World.SubscribeWorldDisposed(w => Worlds.Remove(w));
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveFromParentsOf(this Entity parent, Entity child)
        {
            if (parent.Has<Children>())
            {
                parent.Get<Children>().Value.Remove(child);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetAsChildOf(this Entity child, Entity parent) => parent.SetAsParentOf(child);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveFromChildrenOf(this Entity child, Entity parent) => parent.RemoveFromParentsOf(child);
    }
}
