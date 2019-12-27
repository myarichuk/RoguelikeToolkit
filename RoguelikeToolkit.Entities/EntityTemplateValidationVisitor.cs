namespace RoguelikeToolkit.Entities
{
    public class EntityTemplateValidationVisitor : EntityTemplateBaseVisitor<bool>
    {
        public bool HasComponentsField { get; set; }
        public bool HasInheritsField { get; set; }
        public bool HasIdentifierField { get; set; }

        public void Reset()
        {
            HasComponentsField = false;
            HasIdentifierField = false;
            HasInheritsField = false;
        }

        public override bool VisitIdentifierField(EntityTemplateParser.IdentifierFieldContext context)
        {
            HasIdentifierField = true;
            return base.VisitIdentifierField(context);
        }

        public override bool VisitInheritsField(EntityTemplateParser.InheritsFieldContext context)
        {
            HasInheritsField = true;
            return base.VisitInheritsField(context);
        }

        public override bool VisitComponentsField(EntityTemplateParser.ComponentsFieldContext context)
        {
            HasComponentsField = true;
            return base.VisitComponentsField(context);
        }

        protected override bool AggregateResult(bool aggregate, bool nextResult) => 
            HasComponentsField && HasIdentifierField && HasInheritsField;
    }
}
