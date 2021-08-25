using System;
using System.Collections.Generic;
using System.Text;
using Utf8Json;
using Xunit;

namespace RoguelikeToolkit.Entities.Tests
{
    public class ComponentTemplateTests
    {
        [Fact]
        public void Class_component_template_should_work()
        {
            var component = new AttributesComponent { Agility = 12, Strength = 23 };
            var template = ComponentTemplate.ParseFromString(JsonSerializer.ToJsonString(component));

            var reconstructedComponent = template.CreateInstance<AttributesComponent>();

            Assert.Equal(component.Agility, reconstructedComponent.Agility);
            Assert.Equal(component.Strength, reconstructedComponent.Strength);
        }

        [Fact]
        public void Struct_component_template_should_work()
        {
            var component = new Attributes2Component { Agility = 12, Strength = 23 };
            var template = ComponentTemplate.ParseFromString(JsonSerializer.ToJsonString(component));

            var reconstructedComponent = template.CreateInstance<Attributes2Component>();

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
            var reconstructedComponent = template.CreateInstance<Attributes4Component>();

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
            var reconstructedComponent = template.CreateInstance<Attributes3Component>();

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
            var reconstructedComponent = template.CreateInstance<Attributes4Component>();

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
            var reconstructedComponent = template.CreateInstance<Attributes5Component>();

            Assert.Equal(component.Agility, reconstructedComponent.Agility);
            Assert.Equal(component.Bar.Num, reconstructedComponent.Bar.Num);
            Assert.Equal(component.Bar.Str, reconstructedComponent.Bar.Str);
        }
    }
}
