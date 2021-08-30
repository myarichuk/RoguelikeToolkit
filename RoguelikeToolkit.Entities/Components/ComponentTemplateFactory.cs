using RoguelikeToolkit.Entities.Components;
using System;
using System.Runtime.CompilerServices;

namespace RoguelikeToolkit.Entities
{
    public struct ComponentFactoryOptions
    {
        public bool IgnoreMissingFields;
        public bool IgnoreInvalidEnumFields;
    }

    public class ComponentFactory
    {
        private readonly ComponentInstanceCreator _creator = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TInstance CreateInstance<TInstance>(ComponentTemplate template) => (TInstance)CreateInstance(typeof(TInstance), template);

        public object CreateInstance(Type type, ComponentTemplate template)
        {
            if (type.IsInterface) //precaution!
            {
                throw new InvalidOperationException($"Cannot create component instance with interface type. (specified type = {type.FullName})");
            }

            if (type == typeof(string) || type.IsPrimitive || type.IsEnum || type.IsPointer || type.IsCOMObject || type.IsByRef)
            {
                throw new InvalidOperationException($"Cannot create component instance with a specified type. The type should *not* be a primitive, enum, by-ref type or a pointer (specified type = {type.FullName})");
            }

            var instance = _creator.CreateInstance(type, template.PropertyValues);

            return instance;
        }
    }
}
