using Microsoft.Extensions.ObjectPool;
using System;

namespace RoguelikeToolkit.Entities
{
    public static class ObjectPoolProvider
    {
        private readonly static Lazy<DefaultObjectPoolProvider> _opp = new Lazy<DefaultObjectPoolProvider>(() => new DefaultObjectPoolProvider());

        public static Microsoft.Extensions.ObjectPool.ObjectPoolProvider Instance => _opp.Value;
    }
}
