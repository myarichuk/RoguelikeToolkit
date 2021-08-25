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
    }
}
