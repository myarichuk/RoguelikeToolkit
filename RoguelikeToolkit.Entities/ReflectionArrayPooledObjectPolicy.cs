using System.Collections.Concurrent;
using Microsoft.Extensions.ObjectPool;

namespace RoguelikeToolkit.Entities
{
    internal class ReflectionParamsPooledObjectPolicy<TItem> : IPooledObjectPolicy<TItem[]>
    {
        private readonly ConcurrentQueue<TItem[]> _pool = new();
        private readonly int _arraySize;

        public ReflectionParamsPooledObjectPolicy(int arraySize) => _arraySize = arraySize;

        public TItem[] Create() =>
            _pool.TryDequeue(out var array) ? array : (new TItem[_arraySize]);

        public bool Return(TItem[] array)
        {
            _pool.Enqueue(array);
            return true;
        }
    }
}