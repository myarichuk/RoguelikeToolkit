using System;
using Antlr4.Runtime;
using Xunit;

namespace RoguelikeToolkit.Entities.Tests
{
    public class EntityTemplateParsingTests
    {
        [Fact]
        public void CanVerifyMandatoryFields()
        {
            //with mandatory fields
            var template1 = @"
            {
              ""Id"": ""object"",
              ""Inherits"": [],
              ""Components"": [
                { ""Health"": 100.0 },
                { ""Weight"": 0.0 }
              ]
            }
            ";

            //with some mandatory fields missing
            var template2 = @"
            {
              ""Id"": ""object"",
              ""Components"": [
                { ""Health"": 100.0 },
                { ""Weight"": 0.0 }
              ]
            }
            ";

            //with all mandatory fields missing
            var template3 = @"
            {
              ""Foo"": ""Bar"",
              ""Bar"": ""Foo""
            }
            ";

            var lexer = new EntityTemplateLexer(new AntlrInputStream(template1));
            var parser = new EntityTemplateParser(new CommonTokenStream(lexer));
            parser.ErrorHandler = new BailErrorStrategy();
            var validationVisitor = new EntityTemplateValidationVisitor();
            var ast1 = parser.template();

            Assert.True(validationVisitor.Visit(ast1));
            Assert.True(validationVisitor.HasComponentsField);
            Assert.True(validationVisitor.HasIdentifierField);
            Assert.True(validationVisitor.HasInheritsField);

            lexer.SetInputStream(new AntlrInputStream(template2));
            parser.SetInputStream(new CommonTokenStream(lexer));
            var ast2 = parser.template();
            validationVisitor.Reset();
            Assert.False(validationVisitor.Visit(ast2));
            Assert.True(validationVisitor.HasComponentsField);
            Assert.True(validationVisitor.HasIdentifierField);
            Assert.False(validationVisitor.HasInheritsField);

            lexer.SetInputStream(new AntlrInputStream(template3));
            parser.SetInputStream(new CommonTokenStream(lexer));
            var ast3 = parser.template();
            validationVisitor.Reset();
            Assert.False(validationVisitor.Visit(ast3));
            Assert.False(validationVisitor.HasComponentsField);
            Assert.False(validationVisitor.HasIdentifierField);
            Assert.False(validationVisitor.HasInheritsField);

        }
    }
}
