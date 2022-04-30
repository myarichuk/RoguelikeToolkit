using System;
using DefaultEcs;
using Jint;
using Jint.Runtime.Interop;

namespace RoguelikeToolkit.Scripts
{
	internal static class EngineFactory
    {
        public static Engine Create(params Type[] typesToReference) => 
            new Engine((_, opt) =>
            {       
                opt.LimitRecursion(1024)
                   .LimitMemory(1024 * 1024)
                   .MaxStatements(500)  
                   .AddExtensionMethods(typeof(EntityGenericsExtensionMethods))
                   //.AddObjectConverter<TypeConverter>()
                   .Configure(engine =>
                   {
                       foreach (var type in typesToReference)
                           engine.SetValue(type.Name, TypeReference.CreateTypeReference(engine, type));

                       engine.SetValue(nameof(Entity), TypeReference.CreateTypeReference(engine, typeof(Entity)));

                   })
                   #if DEBUG
                   .DebugMode()
                   #endif
                   .AllowClr(typeof(Entity).Assembly)
                   .MaxArraySize(1024 * 16);
            });
    }
}
