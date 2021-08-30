using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using FastMember;

namespace RoguelikeToolkit.Entities
{
    public static class MemberAccessor
    {
        private static readonly ConcurrentDictionary<Type, TypeAccessor> AccessorCache = new ConcurrentDictionary<Type, TypeAccessor>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeAccessor Get<T>() => Get(typeof(T));

        public static TypeAccessor Get(Type type)
        {
            if (AccessorCache.TryGetValue(type, out var accessor))
                return accessor;

            var newAccessor = TypeAccessor.Create(type, true);
            AccessorCache.TryAdd(type, newAccessor);
            return newAccessor;
        }
    }
}
