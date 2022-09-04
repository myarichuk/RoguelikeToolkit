using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DefaultEcs;
using Microsoft.Extensions.ObjectPool;
using System;
using RoguelikeToolkit.Scripts;
using RoguelikeToolkit.Entities.Components;

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
			if (!entity.Has<Children>())
				return;

			var children = entity.Get<Children>().Value;
			entity.Remove<Children>();

			foreach (var child in children)
			{
				if (child.IsAlive)
				{
					child.Dispose();
				}
			}
		}

		public static void ExecuteScriptFrom<TComponent>(this ref Entity entity, Func<TComponent, EntityScript> scriptSelector, params (string instanceName, dynamic value)[] @params)
		{
			if (!entity.Has<TComponent>())
				return;

			var script = scriptSelector(entity.Get<TComponent>());

			script.ExecuteOn(ref entity, @params);
		}

		public static void ExecuteScriptFrom<TComponent>(this ref Entity entity, Func<TComponent, EntityComponentScript> scriptSelector, params (string instanceName, dynamic value)[] @params)
		{
			if (!entity.Has<TComponent>())
				return;

			var script = scriptSelector(entity.Get<TComponent>());

			script.TryExecuteOn<TComponent>(ref entity, @params);
		}

		public static void ExecuteScriptFrom<TComponent>(this ref Entity source, ref Entity target, Func<TComponent, EntityInteractionScript> scriptSelector, params (string instanceName, dynamic value)[] @params)
		{
			if (!source.Has<TComponent>())
				return;

			var script = scriptSelector(source.Get<TComponent>());

			script.ExecuteOn(ref source, ref target, @params);
		}

		public static string Id(this Entity entity) =>
			entity.TryGet<IdComponent>(out var id) ? id.Value : null;

		public static ISet<string> Tags(this Entity entity) =>
			entity.TryGet<TagsComponent>(out var metadata) ? metadata.Value : EmptyMetadata;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool HasTags(this Entity entity, IEnumerable<string> tags) => entity.Tags().IsSupersetOf(tags);


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool HasTags(this Entity entity, string tag) => entity.Tags().Contains(tag);

		public static IEnumerable<Entity> GetChidrenWithTags(this Entity parent, params string[] tags)
		{
			if (!parent.Has<Children>())
			{
				yield break;
			}

			foreach (var child in parent.GetChildren())
			{
				if (!child.HasTags(tags))
					continue;

				yield return child;
				foreach (var childOfChild in child.GetChidrenWithTags(tags))
					yield return childOfChild;
			}
		}

		public static bool TryGet<T>(this Entity entity, out T component)
		{
			component = default;
			if (!entity.Has<T>())
				return false;

			component = entity.Get<T>();
			return true;

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
				parent.Get<Children>().Value.Remove(child);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SetAsChildOf(this Entity child, Entity parent) => parent.SetAsParentOf(child);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RemoveFromChildrenOf(this Entity child, Entity parent) => parent.RemoveFromParentsOf(child);
	}
}
