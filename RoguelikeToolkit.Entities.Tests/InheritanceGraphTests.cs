using System;
using System.Linq;
using Xunit;

namespace RoguelikeToolkit.Entities.Tests
{
    public class InheritanceGraphTests
    {
        [Fact]
        public void Inheritance_chain_should_keep_inheritance_order()
        {
            var root = EntityTemplate.ParseFromString(A);
            var childA = EntityTemplate.ParseFromString(B);
            var childB = EntityTemplate.ParseFromString(C);

            var graph = new InheritanceGraph(new []{ root, childB, childA });
            
            var inheritanceChain = graph.GetInheritanceChainFor(childB).ToList();

            Assert.Equal(3, inheritanceChain.Count); //sanity check

            //the order should be in reverse of 
            Assert.Equal(childB.Id, inheritanceChain[0].Id);
            Assert.Equal(childA.Id, inheritanceChain[1].Id);
            Assert.Equal(root.Id, inheritanceChain[2].Id);
        }

        [Fact]
        public void Inheritance_chain_with_cycles_should_throw()
        {
            var a = EntityTemplate.ParseFromString(A); //we need *some* roots to pass sanity check validation
            var x1 = EntityTemplate.ParseFromString(X1);
            var x2 = EntityTemplate.ParseFromString(X2);
            var x3 = EntityTemplate.ParseFromString(X3);

            var graph = new InheritanceGraph(new[] { x1, x2, x3, a });

            Assert.Throws<InvalidOperationException>(() => graph.GetInheritanceChainFor(x3).ToList());
        }

        #region Simple Hierarchy Templates
        private const string A =
        @"
            {
                ""Id"":""A"",
                ""Inherits"": []
            }
        ";

        private const string B =
        @"
            {
                ""Id"":""B"",
                ""Inherits"": [""A""]
            }
        ";

        private const string C =
        @"
            {
                ""Id"":""C"",
                ""Inherits"": [""B""]
            }
        ";
        #endregion

        #region Inheritance Cycle Templates

        private const string X1 =
        @"
            {
                ""Id"":""X1"",
                ""Inherits"": [ ""X3"" ]
            }
        ";

        private const string X2 =
        @"
            {
                ""Id"":""X2"",
                ""Inherits"": [ ""X1"" ]
            }
        ";

        private const string X3 =
        @"
            {
                ""Id"":""X3"",
                ""Inherits"": [ ""X2"" ]
            }
        ";

        #endregion
    }
}
