using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using DefaultEcs;
using Microsoft.Extensions.ObjectPool;

namespace RoguelikeToolkit.Scripts
{
    internal static class ObjectPoolProvider
    {
        private static readonly Lazy<DefaultObjectPoolProvider> TheProvider = new Lazy<DefaultObjectPoolProvider>(() => new DefaultObjectPoolProvider());

        public static Microsoft.Extensions.ObjectPool.ObjectPoolProvider Instance => TheProvider.Value;
    }

    internal class ReflectionParamsPooledObjectPolicy<TItem> : IPooledObjectPolicy<TItem[]>
    {
        private readonly ConcurrentQueue<TItem[]> _pool = new();
        private readonly int _arraySize;

        public ReflectionParamsPooledObjectPolicy()
        {
        }

        public ReflectionParamsPooledObjectPolicy(int arraySize) => _arraySize = arraySize;

        public TItem[] Create() =>
            _pool.TryDequeue(out var array) ? array : (new TItem[_arraySize]);

        public bool Return(TItem[] array)
        {
            _pool.Enqueue(array);
            return true;
        }
    }

    internal static class EntityGenericsExtensionMethods
    {
        private static readonly MethodInfo EntityGet = typeof(Entity).GetMethod(nameof(Entity.Get));
        private static readonly MethodInfo EntitySet = typeof(Entity).GetMethods()
									  .First(m =>
											m.Name == nameof(Entity.Set) &&
											m.GetParameters().Length == 1);
		private static readonly MethodInfo EntityHas = typeof(Entity).GetMethod(nameof(Entity.Has));

		private static readonly ConcurrentDictionary<Type, MethodInfo> GenericGetCache = new ConcurrentDictionary<Type, MethodInfo>();
        private static readonly ConcurrentDictionary<Type, MethodInfo> GenericSetCache = new ConcurrentDictionary<Type, MethodInfo>();
        private static readonly ConcurrentDictionary<Type, MethodInfo> GenericHasCache = new ConcurrentDictionary<Type, MethodInfo>();

        private static readonly ObjectPool<object[]> ParamArrayPool = ObjectPoolProvider.Instance.Create<object[]>(new ReflectionParamsPooledObjectPolicy<object>(1));

        public static bool HasComponent(this Entity entity, Type type)
        {
            var hasMethod = GenericHasCache.GetOrAdd(type, t => EntityHas.MakeGenericMethod(t));
            return (bool)hasMethod.Invoke(entity, Array.Empty<object>());
        }

        public static object GetComponent(this Entity entity, Type type)
        {
            var getMethod = GenericGetCache.GetOrAdd(type, t => EntityGet.MakeGenericMethod(t));
            return getMethod.Invoke(entity, Array.Empty<object>());
        }


        public static void SetComponent(this Entity entity, Type type, object value)
        {
            var setMethod = GenericSetCache.GetOrAdd(type, t => EntitySet.MakeGenericMethod(t));
            object[] @params = null;
            try
            {
                @params = ParamArrayPool.Get();
                @params[0] = value;
                setMethod.Invoke(entity, @params);
            }
            finally
            {
                if (@params != null)
                    ParamArrayPool.Return(@params);
            }
        }

    }
}
