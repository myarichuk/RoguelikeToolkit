using System.Linq;
using Antlr4.Runtime;
using Xunit;

namespace RoguelikeToolkit.Entities.Tests
{
    public class EntityTemplateParsingTests
    {
        [Fact]
        public void CanParseTemplate()
        {
            var template = @"
            {
              ""Id"": ""object"",
              ""ChildEntity"": { ""Components"": { ""Health"": 100.0 } },
              ""Components"": {
                ""Health"": 100.0,
                ""Weight"": 0.5
              },
              ""Inherits"": [""A""]              
            }
            ";

            var lexer = new EntityTemplateLexer(new AntlrInputStream(template));
            var parser = new EntityTemplateParser(new CommonTokenStream(lexer))
            {
                ErrorHandler = new BailErrorStrategy()
            };

            var parserVisitor = new EntityTemplateParserVisitor();
            var ast = parser.template();

            var entityTemplate = parserVisitor.Visit(ast);
            
            Assert.True(entityTemplate.Children.ContainsKey("ChildEntity"));
            Assert.Single(entityTemplate.Children["ChildEntity"].Components);
            Assert.Equal(100.0, entityTemplate.Children["ChildEntity"].Components["Health"]);

            Assert.Equal(2, entityTemplate.Components.Count);
            Assert.Equal(100.0, entityTemplate.Components["Health"]);
            Assert.Equal(0.5, entityTemplate.Components["Weight"]);
        }

        [Fact]
        public void CanDetectDuplicateFieldsInDifferentContexts()
        {
            var template = @"
            {
              ""Id"": ""object"",
              ""Inherits"": [],
              ""Components"": {
                ""Health"": 100.0,
                ""Inherits"": [""A""]
              }
            }
            ";

            var template2 = @"
            {
              ""Id"": ""object"",
              ""Inherits"": [],
              ""Components"": {
                ""Health"": 100.0,
                ""Inherits"": [""A""]
              },
              ""Id"": ""object""
            }
            ";

            var lexer = new EntityTemplateLexer(new AntlrInputStream(template));
            var parser = new EntityTemplateParser(new CommonTokenStream(lexer))
            {
                ErrorHandler = new BailErrorStrategy()
            };

            var validationVisitor = new EntityTemplateValidationVisitor();
            var ast = parser.template();

            Assert.True(validationVisitor.Visit(ast));

            Assert.Empty(validationVisitor.DuplicateFields);
            lexer.SetInputStream(new AntlrInputStream(template2));
            parser.SetInputStream(new CommonTokenStream(lexer));
            var ast2 = parser.template();
            validationVisitor.Reset();
            Assert.False(validationVisitor.Visit(ast2));
            Assert.Equal(2, validationVisitor.DuplicateFields.Count());
            Assert.Contains(validationVisitor.DuplicateFields, x => x == "Health" || x == "Id");
        }

        [Fact]
        public void CanDetectDuplicateFields()
        {
            //template with duplicate fields
            var template = @"
            {
              ""Inherits"": [],
              ""Components"": {
                ""Health"": 100.0,
                ""Inherits"": [""A""]
              },
              ""Inherits"": [""A""]              
            }
            ";

            var lexer = new EntityTemplateLexer(new AntlrInputStream(template));
            var parser = new EntityTemplateParser(new CommonTokenStream(lexer))
            {
                ErrorHandler = new BailErrorStrategy()
            };

            var validationVisitor = new EntityTemplateValidationVisitor();
            var ast = parser.template();

            Assert.False(validationVisitor.Visit(ast));
            Assert.Single(validationVisitor.DuplicateFields);
            Assert.Contains(validationVisitor.DuplicateFields, x => x == "Inherits");
        }

        [Fact]
        public void CanDetectNonObjectNonMandatoryRootFields()
        {
            //template with duplicate fields
            var template = @"
            {
              ""Inherits"": [],
              ""Components"": {
                ""Health"": 100.0,
                ""Inherits"": [""A""]
              },
              ""Foo"": 100.0,
              ""FooBar"" : { ""Health"": 100.0 },
              ""Bar"": [""A""]              
            }
            ";

            var lexer = new EntityTemplateLexer(new AntlrInputStream(template));
            var parser = new EntityTemplateParser(new CommonTokenStream(lexer))
            {
                ErrorHandler = new BailErrorStrategy()
            };

            var validationVisitor = new EntityTemplateValidationVisitor();
            var ast = parser.template();

            Assert.False(validationVisitor.Visit(ast));
            Assert.Equal(2, validationVisitor.NonObjectFieldKeys.Count());
            Assert.Contains(validationVisitor.NonObjectFieldKeys, x => x == "Foo" || x == "Bar");
        }

        [Fact]
        public void CanVerifyMandatoryFields()
        {
            //with mandatory fields
            var template1 = @"
            {
              ""Id"": ""object"",
              ""Inherits"": [],
              ""Components"": {
                ""Health"": 100.0,
                ""Inherits"": [""A""]
              }
            }
            ";

            //with some mandatory fields missing
            var template2 = @"
            {
              ""Id"": ""object"",
              ""Components"": {
                ""Health"": 100.0,
                ""Inherits"": [""A""]
              }
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
            var parser = new EntityTemplateParser(new CommonTokenStream(lexer))
            {
                ErrorHandler = new BailErrorStrategy()
            };
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
