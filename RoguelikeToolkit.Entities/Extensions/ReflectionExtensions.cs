using System;
using System.Reflection;

namespace RoguelikeToolkit.Entities
{
    public static class ReflectionExtensions
    {
        public static Type GetUnderlyingType(this MemberInfo member) => 
            member.MemberType switch
            {
                MemberTypes.Event => ((EventInfo)member).EventHandlerType,
                MemberTypes.Field => ((FieldInfo)member).FieldType,
                MemberTypes.Method => ((MethodInfo)member).ReturnType,
                MemberTypes.Property => ((PropertyInfo)member).PropertyType,
                _ => throw new ArgumentException("Input MemberInfo must be if type EventInfo, FieldInfo, MethodInfo, or PropertyInfo"),
            };
    }
}
