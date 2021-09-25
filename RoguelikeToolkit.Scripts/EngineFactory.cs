using System;
using System.Reflection;
using DefaultEcs;
using Jint;
using Jint.Native;
using Jint.Runtime.Interop;

namespace RoguelikeToolkit.Scripts
{
    //public class TypeConverter : IObjectConverter
    //{
    //    public bool TryConvert(Engine engine, object value, out JsValue result)
    //    {
    //        result = default;

    //        if(!(value is Type))
    //            return false;

    //        result = JsValue.FromObject(engine, value);

    //        return true;
    //    }
    //}

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

//        public static Engine Create(Type[] typesToReference, params Assembly[] referenceAssemblies) =>
//            new Engine((_, opt) =>
//            {
//                opt.LimitRecursion(1024)
//                   .LimitMemory(1024 * 1024)
//                   .MaxStatements(500)
//                   .Configure(engine =>
//                   {
//                       foreach (var type in typesToReference)
//                           engine.SetValue(type.Name, TypeReference.CreateTypeReference(engine, type));

//                       engine.SetValue(nameof(Entity), TypeReference.CreateTypeReference(engine, typeof(Entity)));

//                   })
//#if DEBUG
//                   .DebugMode()
//#endif
//                   .AllowClr(referenceAssemblies);
//            });

    }
}
