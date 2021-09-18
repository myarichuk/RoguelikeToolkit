using System;
using Microsoft.Extensions.ObjectPool;

namespace RoguelikeToolkit.Entities
{
    public static class ObjectPoolProvider
    {
        private static readonly Lazy<DefaultObjectPoolProvider> TheProvider = new Lazy<DefaultObjectPoolProvider>(() => new DefaultObjectPoolProvider());

        public static Microsoft.Extensions.ObjectPool.ObjectPoolProvider Instance => TheProvider.Value;
    }
}
