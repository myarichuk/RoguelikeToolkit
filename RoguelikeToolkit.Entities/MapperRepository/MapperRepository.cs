using System;
using System.Collections.Generic;
using System.Linq;
using RoguelikeToolkit.Entities.Components.TypeMappers;

namespace RoguelikeToolkit.Entities
{
    public class MapperRepository
    {
        private readonly List<ITypeMapper> _typeMappers = new();
        private readonly List<IPropertyMapper> _propertyMappers = new();

        public IReadOnlyList<ITypeMapper> TypeMappers => _typeMappers;
        public IReadOnlyList<IPropertyMapper> PropertyMappers => _propertyMappers;

        public MapperRepository(params IMapperResolver[] resolvers)
        {
            foreach(var resolver in resolvers)
                RegisterFrom(resolver);
        }

        public void RegisterFrom(IMapperResolver mapperResolver)
        {
            var newTypeMappers = 
                from type in mapperResolver.GetTypeMappers()
                where type.ImplementsInterface<ITypeMapper>() && 
                        !type.IsInterface && !type.IsAbstract && !type.IsCOMObject //just in case
                select (ITypeMapper)type.CreateInstance();

            _typeMappers.AddRange(newTypeMappers);
            _typeMappers.Sort((first, second) => first.Priority.CompareTo(second.Priority));

            var newPropertyMappers =
                from type in mapperResolver.GetPropertyMappers()
                where type.ImplementsInterface<IPropertyMapper>() 
                    && !type.IsInterface && !type.IsAbstract && !type.IsCOMObject //just in case
                select (IPropertyMapper)type.CreateInstance();

            _propertyMappers.AddRange(newPropertyMappers);
            _propertyMappers.Sort((first, second) => first.Priority.CompareTo(second.Priority));
        }

        public void Register(ITypeMapper typeMapper)
        {
            _typeMappers.Add(typeMapper);
            _typeMappers.Sort((first, second) => first.Priority.CompareTo(second.Priority));
        }

        public void Register(IPropertyMapper propertyMapper)
        {
            _propertyMappers.Add(propertyMapper);
            _propertyMappers.Sort((first, second) => first.Priority.CompareTo(second.Priority));
        }

        public void RegisterFromType(Type mapperType)
        {
            var interfaces = mapperType.GetInterfaces();

            if (interfaces.Contains(typeof(IPropertyMapper)))
            {
                _propertyMappers.Add((IPropertyMapper)mapperType.CreateInstance());
                _propertyMappers.Sort((first, second) => first.Priority.CompareTo(second.Priority));
            }
            else if (interfaces.Contains(typeof(ITypeMapper)))
            {
                _typeMappers.Add((ITypeMapper)mapperType.CreateInstance());
                _typeMappers.Sort((first, second) => first.Priority.CompareTo(second.Priority));
            }
            else
                throw new NotSupportedException($"A type supplied to MapperRepository::RegisterFromType() should implement either {nameof(IPropertyMapper)} or {nameof(ITypeMapper)}");
        }
    }
}
