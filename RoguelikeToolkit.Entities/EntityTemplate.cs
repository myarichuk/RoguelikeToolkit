using System;
using System.Collections.Generic;
using Antlr4.Runtime;

namespace RoguelikeToolkit.Entities
{
    public class EntityTemplate : IEquatable<EntityTemplate>
    {
        private static readonly EntityTemplateValidatorVisitor ValidatorVisitor = new EntityTemplateValidatorVisitor();
        public string Id { get; set; }
        public HashSet<string> Inherits { get; set; } = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        public Dictionary<string, EntityTemplate> Children { get; set; } = new Dictionary<string, EntityTemplate>(StringComparer.InvariantCultureIgnoreCase);
        public Dictionary<string, object> Components { get; set; } = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);

        public static EntityTemplate Parse(string templateJson, IAntlrErrorStrategy errorStrategy = null)
        {
            var lexer = new EntityTemplateLexer(new AntlrInputStream(templateJson));
            var parser = new EntityTemplateParser(new CommonTokenStream(lexer))
            {
                ErrorHandler = errorStrategy ?? new BailErrorStrategy()
            };

            var ast = parser.template();

            ValidatorVisitor.Reset();
            ValidatorVisitor.Visit(ast);
            ValidatorVisitor.ThrowExceptionIfErrors();

            var parserVisitor = new EntityTemplateParserVisitor();
            return parserVisitor.Visit(ast);
        }

        #region IEquatable Implementation

        public bool Equals(EntityTemplate other) =>
            !ReferenceEquals(null, other) && (ReferenceEquals(this, other) ||
                                              string.Equals(Id, other.Id,
                                                  StringComparison.InvariantCultureIgnoreCase));

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;

            return obj.GetType() == this.GetType() && Equals((EntityTemplate) obj);
        }

        public override int GetHashCode() => (Id != null ? Id.GetHashCode() : 0);

        public static bool operator ==(EntityTemplate left, EntityTemplate right) => Equals(left, right);
        public static bool operator !=(EntityTemplate left, EntityTemplate right) => !Equals(left, right);

        #endregion
    }
}
