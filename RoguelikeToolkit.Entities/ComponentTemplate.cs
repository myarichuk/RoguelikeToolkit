﻿using FastMember;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Utf8Json;

namespace RoguelikeToolkit.Entities
{
    public class ComponentTemplate
    {
        private readonly IReadOnlyDictionary<string, object> _propertyValues;
        private readonly static ConcurrentDictionary<Type, Dictionary<string, MemberInfo>> _membersCache = new ConcurrentDictionary<Type, Dictionary<string, MemberInfo>>();

        internal ComponentTemplate(IReadOnlyDictionary<string, object> propertyValues) => 
            _propertyValues = InitializeEmbeddedTemplates(propertyValues);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ComponentTemplate ParseFromFile(string jsonFile)
        {
            if (!File.Exists(jsonFile))
                ThrowNotFound(jsonFile);

            //not very efficient (memory allocations!) but more efficiency is unlikely to be needed *here*
            return ParseFromString(File.ReadAllText(jsonFile));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ComponentTemplate ParseFromString(string json)
        {
            //note: Utf8Json embeds IReadOnlyDictionary<string, object> for embedded objects
            // *IF* it is set to deserialize as *dynamic*
            var data = (IReadOnlyDictionary<string, object>)JsonSerializer.Deserialize<dynamic>(json);
            return new ComponentTemplate(data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TInstance CreateInstance<TInstance>() =>
            (TInstance)CreateInstance(typeof(TInstance));

        public object CreateInstance(Type type)
        {
            var typeAccessor = MemberAccessor.Get(type);
            var instance = typeAccessor.CreateNew();
            
            ApplyPropertyValuesRecursive(instance, typeAccessor);

            return instance;
        }

        #region Helpers

        private static void ThrowNotFound(string jsonFile) => 
            throw new ArgumentException($"The file {jsonFile} was not found", nameof(jsonFile));

        private void ApplyPropertyValuesRecursive(object instance, TypeAccessor typeAccessor)
        {
            var type = instance.GetType();
            var membersByName = _membersCache.GetOrAdd(type, 
                newType => 
                    newType.GetMembers()
                           .Where(m => m.MemberType == MemberTypes.Field || m.MemberType == MemberTypes.Property)
                           .ToDictionary(m => m.Name, m => m));

            foreach (var prop in _propertyValues)
            {
                if (!membersByName.TryGetValue(prop.Key, out var member))
                    throw new InvalidDataException($"Field or property by name of {prop.Key} wasn't found in the type. This is probably a misuse or you are using an incorrect type");
                var memberType = member.GetUnderlyingType();

                typeAccessor[instance, prop.Key] = 
                    prop.Value switch
                    {
                        ComponentTemplate embeddedTemplate => embeddedTemplate.CreateInstance(memberType),
                        _ => Convert.ChangeType(prop.Value, memberType), //TODO: don't forget to test limits of this ChangeType, edge cases, etc
                    };
            }
        }

        private static IReadOnlyDictionary<string, object> InitializeEmbeddedTemplates(IReadOnlyDictionary<string, object> data)
        {
            var dataWithEmbeddedTemplates = new Dictionary<string, object>();
            foreach (var item in data)
            {
                switch (item.Value)
                {
                    case IReadOnlyDictionary<string, object> embedded:
                        dataWithEmbeddedTemplates.Add(item.Key, new ComponentTemplate(embedded));
                        break;
                    default:
                        dataWithEmbeddedTemplates.Add(item.Key, item.Value);
                        break;
                }
            }
            return dataWithEmbeddedTemplates;
        }

        #endregion
    }
}