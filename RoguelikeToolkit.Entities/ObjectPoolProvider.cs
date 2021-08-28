using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Concurrent;

namespace RoguelikeToolkit.Entities
{
    public static class ObjectPoolProvider
    {
        private readonly static Lazy<DefaultObjectPoolProvider> _opp = new Lazy<DefaultObjectPoolProvider>(() => new DefaultObjectPoolProvider());

        public static Microsoft.Extensions.ObjectPool.ObjectPoolProvider Instance => _opp.Value;
    }

    internal class ThreadSafeObjectPoolPolicy<TObject> : IPooledObjectPolicy<TObject> where TObject : class, new()
    {
        private readonly ConcurrentQueue<TObject> _pool = new();

        public TObject Create()
        {
            if (_pool.TryDequeue(out var instance))
                return instance;

            return new TObject();
        }

        public bool Return(TObject obj)
        {
            _pool.Enqueue(obj);
            return true;
        }
    }
}
