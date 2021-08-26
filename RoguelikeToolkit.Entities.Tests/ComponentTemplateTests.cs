using System;
using System.IO;
using Utf8Json;
using Xunit;

namespace RoguelikeToolkit.Entities.Tests
{
    public interface SomeInteface { }
    public interface SomeIntefaceWithGenerics<T> { }
    public enum SomeEnum { One, Two, Three }

    public class ComponentTemplateTests
    {
        private readonly ComponentFactory _factory = new();

        [Fact]
        public void Should_throw_if_invalid_json() =>
            Assert.Throws<InvalidDataException>(() => ComponentTemplate.ParseFromString("This is not json!!"));

        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(SomeInteface))]
        [InlineData(typeof(SomeIntefaceWithGenerics<>))]
        [InlineData(typeof(SomeIntefaceWithGenerics<char>))]
        [InlineData(typeof(int))]
        [InlineData(typeof(void*))]
        [InlineData(typeof(SomeEnum))]
        public void Should_throw_if_trying_to_build_interface_with_unsupported_type(Type componentType) =>
            Assert.Throws<InvalidOperationException>(() => 
                _factory.CreateInstance(componentType, ComponentTemplate.ParseFromString("{}")));

        [Fact]
        public void Class_component_template_should_work()
        {
            var component = new AttributesComponent { Agility = 12, Strength = 23 };
            var template = ComponentTemplate.ParseFromString(JsonSerializer.ToJsonString(component));

            var reconstructedComponent = _factory.CreateInstance<AttributesComponent>(template);

            Assert.Equal(component.Agility, reconstructedComponent.Agility);
            Assert.Equal(component.Strength, reconstructedComponent.Strength);
        }

        [Fact]
        public void Dynamic_component_type_should_throw()
        {
            var component = new AttributesComponent { Agility = 12, Strength = 23 };
            var template = ComponentTemplate.ParseFromString(JsonSerializer.ToJsonString(component));

            Assert.Throws<InvalidDataException>(() => _factory.CreateInstance<dynamic>(template));
        }

        [Fact]
        public void Struct_component_template_should_work()
        {
            var component = new Attributes2Component { Agility = 12, Strength = 23 };
            var template = ComponentTemplate.ParseFromString(JsonSerializer.ToJsonString(component));

            var reconstructedComponent = _factory.CreateInstance<Attributes2Component>(template);

            Assert.Equal(component.Agility, reconstructedComponent.Agility);
            Assert.Equal(component.Strength, reconstructedComponent.Strength);
        }

        [Fact]
        public void Duck_typing_should_work()
        {
            var component = new Attributes3Component
            {
                Agility = 12,
                Bar = new Foo
                {
                    Num = 234,
                    Str = "ABC"
                }
            };

            var template = ComponentTemplate.ParseFromString(JsonSerializer.ToJsonString(component));
            var reconstructedComponent = _factory.CreateInstance<Attributes4Component>(template);

            Assert.Equal(component.Agility, reconstructedComponent.Agility);
            Assert.Equal(component.Bar.Num, reconstructedComponent.Bar.Num);
            Assert.Equal(component.Bar.Str, reconstructedComponent.Bar.Str);

        }

        [Fact]
        public void Can_build_template_with_class_and_embedded_object_class()
        {
            var component = new Attributes3Component
            {
                Agility = 12,
                Bar = new Foo
                {
                    Num = 234,
                    Str = "ABC"
                }
            };

            var template = ComponentTemplate.ParseFromString(JsonSerializer.ToJsonString(component));
            var reconstructedComponent = _factory.CreateInstance<Attributes3Component>(template);

            Assert.Equal(component.Agility, reconstructedComponent.Agility);
            Assert.Equal(component.Bar.Num, reconstructedComponent.Bar.Num);
            Assert.Equal(component.Bar.Str, reconstructedComponent.Bar.Str);

        }

        [Fact]
        public void Can_build_template_with_struct_and_embedded_object_class()
        {
            var component = new Attributes4Component
            {
                Agility = 12,
                Bar = new Foo
                {
                    Num = 234,
                    Str = "ABC"
                }
            };

            var template = ComponentTemplate.ParseFromString(JsonSerializer.ToJsonString(component));
            var reconstructedComponent = _factory.CreateInstance<Attributes4Component>(template);

            Assert.Equal(component.Agility, reconstructedComponent.Agility);
            Assert.Equal(component.Bar.Num, reconstructedComponent.Bar.Num);
            Assert.Equal(component.Bar.Str, reconstructedComponent.Bar.Str);
        }

        [Fact]
        public void Can_build_template_with_struct_and_embedded_object_struct()
        { 
            var component = new Attributes5Component
            {
                Agility = 12,
                Bar = new Foo2
                {
                    Num = 234,
                    Str = "ABC"
                }
            };

            var template = ComponentTemplate.ParseFromString(JsonSerializer.ToJsonString(component));
            var reconstructedComponent = _factory.CreateInstance<Attributes5Component>(template);

            Assert.Equal(component.Agility, reconstructedComponent.Agility);
            Assert.Equal(component.Bar.Num, reconstructedComponent.Bar.Num);
            Assert.Equal(component.Bar.Str, reconstructedComponent.Bar.Str);
        }

        [Fact]
        public void Can_handle_null_field_values()
        {
            var component = new Foo2
            {
                Num = 234,
                Str = null
            };

            var template = ComponentTemplate.ParseFromString(JsonSerializer.ToJsonString(component));
            var reconstructedComponent = _factory.CreateInstance<Foo2>(template);

            Assert.Equal(component.Num, reconstructedComponent.Num);
            Assert.Null(reconstructedComponent.Str);
        }
    }
}
