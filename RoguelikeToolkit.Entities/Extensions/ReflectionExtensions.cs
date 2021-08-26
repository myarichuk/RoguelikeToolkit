using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        public static bool TryCast<T>(this object obj, out T objAfterCast)
        {
            objAfterCast = default;
            try
            {
                objAfterCast = (T)Convert.ChangeType(obj, typeof(T));
                return true;
            }
            catch (OverflowException)
            {
                return false;
            }
            catch (FormatException)
            {
                return false;
            }
            catch (InvalidCastException)
            {
                return false;
            }
        }
    }
}
