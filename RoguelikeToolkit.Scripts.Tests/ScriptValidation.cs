using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RoguelikeToolkit.Scripts.Tests
{
    public class ScriptValidation
    {
        [Fact]
        public void Script_with_invalid_using_clause_should_throw()
        {
            var script = "using System.IO; Console.WriteLine(\"Hello World\");";
            Assert.Throws<InvalidOperationException>(() => ScriptFactory.CreateCompiled<object>(script));
        }
    }
}
