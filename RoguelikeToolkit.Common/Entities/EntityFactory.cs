using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization;
using DefaultEcs;
using FastMember;
using RoguelikeToolkit.Common.EntityTemplates;

namespace RoguelikeToolkit.Common.Entities
{
    public class EntityFactory
    {
        private static readonly Dictionary<string, Type> _componentTypes;
        private static readonly Dictionary<Type, TypeAccessor> _typeAccessorCache = new Dictionary<Type, TypeAccessor>();
        static EntityFactory()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            var components = 
                (from assembly in assemblies
                 from type in assembly.GetTypes()
                 where type.Name.EndsWith("component", true, CultureInfo.InvariantCulture) ||
                       type.GetInterfaces()
                           .Any(it => it.IsGenericType && it.GetGenericTypeDefinition() == typeof(IValueComponent<>)) ||
                       type.GetInterfaces()
                           .Any(it => it.IsGenericType && it.GetGenericTypeDefinition() == typeof(ICollectionComponent<>)) ||
                       type.GetCustomAttributes(typeof(ComponentAttribute), true).Any()
                 select type).ToArray();

            _componentTypes = components.ToDictionary(x => 
                x.Name.EndsWith("component", true, CultureInfo.InvariantCulture) ? 
                    x.Name.Replace("component", string.Empty, true, CultureInfo.InvariantCulture) : 
                    x.Name, 
                x => x);
        }

        private readonly EntityTemplateRepository _templateRepository;

        public EntityFactory(EntityTemplateRepository templateRepository)
        {
            _templateRepository = templateRepository;
        }

        public bool TryCreate(string templateId, ref Entity entity)
        {
            if (_templateRepository.Templates.ContainsKey(templateId))
            {
                var template = _templateRepository.Templates[templateId];
                ApplyComponents(template, ref entity);
                //TODO: finish here

                void ApplyComponents(EntityTemplate current, ref Entity entity)
                {
                    foreach (var componentData in current.Components)
                    {
                        if (_componentTypes.ContainsKey(componentData.Key))
                        {
                            var componentType = _componentTypes[componentData.Key];
                            var componentInstance =
                                FormatterServices.GetUninitializedObject(componentType);

                            if (!_typeAccessorCache.TryGetValue(componentType, out var typeAccessor))
                            {
                                typeAccessor = TypeAccessor.Create(componentType, true);
                                _typeAccessorCache.Add(componentType, typeAccessor);
                            }

                            //TODO: handle here both primitives and nested objects
                            switch(Type.GetTypeCode(componentData.Value.GetType()))
                            {
                                case TypeCode.Object:
                                    var componentDataValues = (Dictionary<string, object>)componentData.Value;
                                    foreach (var (key, value) in componentDataValues)
                                    {
                                        try
                                        {
                                            typeAccessor[componentInstance, key] = value;
                                        }
                                        catch(ArgumentOutOfRangeException e)
                                        {
                                            //don't error on non-existing properties
                                            //TODO: add logging
                                        }
                                    }
                                    break;
                                default: //primitives and string...
                                    break;
                            }

                        }
                    }
                }

                return true;
            }

            return false;
        }
    }
}
