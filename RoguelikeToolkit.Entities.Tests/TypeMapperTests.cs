using System.Collections.Generic;
using RoguelikeToolkit.Entities.Components.TypeMappers;
using Xunit;

namespace RoguelikeToolkit.Entities.Tests
{
    public class TypeMapperTests
    {
        [Fact]
        public void Can_map_dictionary_to_dynamic()
        {
            var mapper = new DynamicTypeMapper();

            var data = new Dictionary<string, object> { { "foo", 1 }, { "bar", 2 } };

            Assert.True(mapper.CanMap(typeof(object), data));

            dynamic mappedData = mapper.Map(null, typeof(object), data, (_, __) => null);
            Assert.Equal(1, mappedData.foo);
            Assert.Equal(2, mappedData.bar);
        }

        [Fact]
        public void Can_map_complex_dictionary_to_dynamic()
        {
            var mapper = new DynamicTypeMapper();

            var data = new Dictionary<string, object> { { "foo", 1 }, { "bar", 2 }, { "blah", new Dictionary<string, object> { { "A", "B" } } } };

            Assert.True(mapper.CanMap(typeof(object), data));

            dynamic mappedData = mapper.Map(null, typeof(object), data, (_, __) => null);
            Assert.Equal(1, mappedData.foo);
            Assert.Equal(2, mappedData.bar);
            Assert.Equal("B", mappedData.blah.A);
        }
    }
}
