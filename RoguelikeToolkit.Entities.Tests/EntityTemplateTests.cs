using System.IO;
using Xunit;

namespace RoguelikeToolkit.Entities.Tests
{
    public class Bar : IValueComponent<double>
    {
        public double Value {  get; set; }
    }

    public class EntityTemplateTests
    {
        [Fact]
        public void Should_throw_missing_id() => 
            Assert.Throws<InvalidDataException>(() => EntityTemplate.ParseFromString("{}"));

        [Fact]
        public void Can_parse_inherits()
        {
            var template = EntityTemplate.ParseFromString(
                @"
                    {
                        ""Id"":""FooBar"",
                        ""Inherits"": [ ""AA"", ""BB"", ""CC"" ]
                    }
                 ");

            Assert.NotNull(template.Inherits); //sanity check

            Assert.Contains("AA", template.Inherits);
            Assert.Contains("BB", template.Inherits);
            Assert.Contains("CC", template.Inherits);
        }

        [Fact]
        public void Can_parse_components()
        {
            var template = EntityTemplate.ParseFromString(
                @"{
                        ""Id"":""FooBar"",
                        ""Components"": {
                            ""Foo"": {
                                ""Strength"": 1.0,
                                ""Agility"": 1.0
                            },
                            ""Bar"": 1.5
                        }
                   }
                 ");

            Assert.NotEmpty(template.Components);
        }

        [Fact]
        public void Can_parse_empty_components()
        {
            var template = EntityTemplate.ParseFromString(
                @"{
                        ""Id"":""FooBar"",
                        ""Components"": { }
                   }
                 ");

            Assert.Empty(template.Components);
        }

        [Fact]
        public void Should_throw_invalid_components_type() =>
            Assert.Throws<InvalidDataException>(() => EntityTemplate.ParseFromString(
                @"{
                        ""Id"":""FooBar"",
                        ""Components"": ""This is invalid property type...""
                   }
                 "));           

        [Fact]
        public void Can_parse_empty_inherits()
        {
            var template = EntityTemplate.ParseFromString(
                @"
                    {
                        ""Id"":""FooBar"",
                        ""Inherits"": [ ]
                    }
                 ");

            Assert.NotNull(template.Inherits); //sanity check
            Assert.Empty(template.Inherits);
        }

        [Fact]
        public void Invalid_inherits_field_value_should_throw()
        {
            Assert.Throws<InvalidDataException>(() => EntityTemplate.ParseFromString(
                @"
                    {
                        ""Id"":""FooBar"",
                        ""Inherits"": 123
                    }
                 "));

            Assert.Throws<InvalidDataException>(() => EntityTemplate.ParseFromString(
                @"
                    {
                        ""Id"":""FooBar"",
                        ""Inherits"": null
                    }
                 "));

            Assert.Throws<InvalidDataException>(() => EntityTemplate.ParseFromString(
                @"
                    {
                        ""Id"":""FooBar"",
                        ""Inherits"": { ""AA"" : 123 }
                    }
                 "));
        }

        [Fact]
        public void Invalid_inherits_invalid_array_item_type_should_throw()
        {
            Assert.Throws<InvalidDataException>(() => EntityTemplate.ParseFromString(
                @"
                    {
                        ""Id"":""FooBar"",
                        ""Inherits"": [ ""AA"", 123, ""CC"" ]
                    }
                 "));

            Assert.Throws<InvalidDataException>(() => EntityTemplate.ParseFromString(
                @"
                    {
                        ""Id"":""FooBar"",
                        ""Inherits"": [ ""AA"", ""CC"", 123 ]
                    }
                 "));
        }
    }
}
