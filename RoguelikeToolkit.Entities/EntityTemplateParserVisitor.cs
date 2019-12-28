using System.Collections.Generic;
using System.IO;
using Utf8Json;

namespace RoguelikeToolkit.Entities
{
    public class EntityTemplateParserVisitor : EntityTemplateBaseVisitor<EntityTemplate>
    {
        protected override EntityTemplate DefaultResult { get; } = new EntityTemplate();

        public override EntityTemplate VisitIdentifierField(EntityTemplateParser.IdentifierFieldContext context)
        {
            DefaultResult.Id = context.id.Text[1..^1]; //remove quotes from start and end of the token
            return DefaultResult;
        }

        public override EntityTemplate VisitInheritsField(EntityTemplateParser.InheritsFieldContext context)
        {
            foreach (var inherited in context._baseTemplates)
                DefaultResult.Inherits.Add(inherited.Text[1..^1]); //remove quotes as well

            return DefaultResult;
        }

        public override EntityTemplate VisitComponentsField(EntityTemplateParser.ComponentsFieldContext context)
        {
            var componentData = JsonSerializer.Deserialize<Dictionary<string,object>>(context.componentsObject.GetText());
            DefaultResult.Components = componentData;
            return DefaultResult;
        }

        public override EntityTemplate VisitRegularField(EntityTemplateParser.RegularFieldContext context)
        {
            //we only care about entity template child objects, since they define entity hierarchy.
            if (context.value().@object() == null)
                throw new InvalidDataException(
                    $"Expected from the field '{context.key.Text[1..^1]}' to be an object but it wasn't");

            var childParserVisitor = new EntityTemplateParserVisitor();
            var childEntityTemplate = childParserVisitor.Visit(context.value().@object());

            var childParserValidator = new EntityTemplateValidationVisitor();
            childParserValidator.Visit(context.value().@object());
            if (!childParserValidator.HasIdentifierField)
                childEntityTemplate.Id = context.key.Text[1..^1];

            return !DefaultResult.Children.TryAdd(context.key.Text[1..^1], childEntityTemplate)
                ? throw new InvalidDataException(
                    $"Found duplicate child entity template field. ('{context.key.Text[1..^1]}'). Did you execute template validation visitor?")
                : DefaultResult;

        }

        public override EntityTemplate VisitObject(EntityTemplateParser.ObjectContext context)
        {
            return base.VisitObject(context);
        }

        public override EntityTemplate VisitField(EntityTemplateParser.FieldContext context)
        {
            return base.VisitField(context);
        }

        public override EntityTemplate VisitArray(EntityTemplateParser.ArrayContext context)
        {
            return base.VisitArray(context);
        }
    }
}
