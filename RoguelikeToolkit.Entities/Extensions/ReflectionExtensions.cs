using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

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

		public static bool ImplementsInterface<TInterface>(this Type type)
		{
			var interfaceType = typeof(TInterface);

			if (!interfaceType.IsInterface)
				ThrowNotAnInterface(interfaceType);

			return interfaceType.IsAssignableFrom(type);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static void ThrowNotAnInterface(Type interfaceType) =>
			throw new InvalidOperationException($"the specified type {interfaceType.FullName} must be an interface");

		public static bool IsNumeric(this Type type) =>
			type == typeof(short) ||
			type == typeof(int) ||
			type == typeof(long) ||
			type == typeof(ushort) ||
			type == typeof(uint) ||
			type == typeof(ulong) ||
			type == typeof(float) ||
			type == typeof(decimal) ||
			type == typeof(double);

		public static bool IsICollection(this Type type) => !type.IsArray &&
					type.GetInterfaces().Any(i =>
						i.IsGenericType &&
						i.GenericTypeArguments.Length == 1 &&
						i.FullName.StartsWith("System.Collections.Generic.ICollection"));

		public static object CreateInstance(this Type type)
		{
			if (type.IsAbstract || type.IsInterface)
				throw new NotSupportedException("Cannot instantiate an interface or an abstract type");

			var accessor = MemberAccessor.Get(type);
			return accessor.CreateNewSupported ? accessor.CreateNew() : FormatterServices.GetUninitializedObject(type);
		}

		public static bool IsDictionary(this Type type) =>
			type.GetInterfaces()
				.Any(i => i.FullName.StartsWith("System.Collections.Generic.IDictionary") &&
							  i.IsGenericType &&
							  i.GenericTypeArguments.Length == 2);

		public delegate object ObjectActivator(params object[] args);

		//adapted from here: https://rogerjohansson.blog/2008/02/28/linq-expressions-creating-objects/
		public static ObjectActivator CreateActivator(this ConstructorInfo ctor)
		{
			var paramsInfo = ctor.GetParameters();

			//create a single param of type object[]
			var param = Expression.Parameter(typeof(object[]), "args");

			var argsExp = new Expression[paramsInfo.Length];

			//pick each arg from the params array 
			//and create a typed expression of them
			for (int i = 0; i < paramsInfo.Length; i++)
			{
				var index = Expression.Constant(i);
				Type paramType = paramsInfo[i].ParameterType;

				var paramAccessorExp = Expression.ArrayIndex(param, index);
				argsExp[i] = Expression.Convert(paramAccessorExp, paramType);
			}

			//make a NewExpression that calls the
			//ctor with the args we just created
			var newExp = Expression.New(ctor, argsExp);

			//create a lambda with the New
			//Expression as body and our param object[] as arg
			var lambda = Expression.Lambda(typeof(ObjectActivator), newExp, param);

			var compiled = (ObjectActivator)lambda.Compile();
			return compiled;
		}
	}
}
