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

            var graph = new InheritanceGraph(new[] { root, childB, childA });

            var inheritanceChain = graph.GetInheritanceChainFor(childB).ToList();

            Assert.Equal(3, inheritanceChain.Count); //sanity check

            //the order should be in reverse of 
            Assert.Equal(childB.Id, inheritanceChain[0].Id);
            Assert.Equal(childA.Id, inheritanceChain[1].Id);
            Assert.Equal(root.Id, inheritanceChain[2].Id);
        }

        [Fact]
        public void Multiple_Inheritance_chain_should_keep_inheritance_order()
        {
            var t1 = EntityTemplate.ParseFromString(T1);
            var t2 = EntityTemplate.ParseFromString(T2);
            var t3 = EntityTemplate.ParseFromString(T3);
            var t4 = EntityTemplate.ParseFromString(T4);

            var graph = new InheritanceGraph(new[] { t1, t3, t2, t4 });

            var inheritanceChain = graph.GetInheritanceChainFor(t4).ToList();

            Assert.Equal(4, inheritanceChain.Count); //sanity check

            //multiple inheritance will treat parents of the same level in-order
            Assert.Equal(t4.Id, inheritanceChain[0].Id);
            Assert.Equal(t1.Id, inheritanceChain[1].Id);
            Assert.Equal(t2.Id, inheritanceChain[2].Id);
            Assert.Equal(t3.Id, inheritanceChain[3].Id);
        }

        [Fact]
        public void Multiple_Inheritance_chain_multiple_levels_should_keep_inheritance_order()
        {
            var t1 = EntityTemplate.ParseFromString(T1);
            var t2 = EntityTemplate.ParseFromString(T2);
            var t3 = EntityTemplate.ParseFromString(T3);
            var t4 = EntityTemplate.ParseFromString(T4);
            var t5 = EntityTemplate.ParseFromString(T5);

            var graph = new InheritanceGraph(new[] { t1, t3, t2, t4, t5 });

            var inheritanceChain = graph.GetInheritanceChainFor(t5).ToList();

            Assert.Equal(5, inheritanceChain.Count); //sanity check

            //multiple inheritance will treat parents of the same level in-order
            Assert.Equal(t5.Id, inheritanceChain[0].Id);
            Assert.Equal(t4.Id, inheritanceChain[1].Id);
            Assert.Equal(t1.Id, inheritanceChain[2].Id);
            Assert.Equal(t2.Id, inheritanceChain[3].Id);
            Assert.Equal(t3.Id, inheritanceChain[4].Id);
        }

        [Fact]
        public void Inheritance_chain_with_cycles_should_traverse_cycle_only_once()
        {
            var a = EntityTemplate.ParseFromString(A); //we need *some* roots to pass sanity check validation
            var x1 = EntityTemplate.ParseFromString(X1);
            var x2 = EntityTemplate.ParseFromString(X2);
            var x3 = EntityTemplate.ParseFromString(X3);

            var graph = new InheritanceGraph(new[] { x1, x2, x3, a });

            var inheritanceChain = graph.GetInheritanceChainFor(x3).ToList();

            Assert.Equal(x3.Id, inheritanceChain[0].Id);
            Assert.Equal(x2.Id, inheritanceChain[1].Id);
            Assert.Equal(x1.Id, inheritanceChain[2].Id);

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

        #region Multiple Inheritance Template

        private const string T1 =
        @"
            {
                ""Id"":""T1"",
                ""Inherits"": []
            }
        ";

        private const string T2 =
        @"
            {
                ""Id"":""T2"",
                ""Inherits"": []
            }
        ";

        private const string T3 =
        @"
            {
                ""Id"":""T3"",
                ""Inherits"": []
            }
        ";

        private const string T4 =
        @"
            {
                ""Id"":""T4"",
                ""Inherits"": [""T1"", ""T2"", ""T3""]
            }
        ";

        private const string T5 =
@"
            {
                ""Id"":""T5"",
                ""Inherits"": [""T4"", ""T1""]
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
