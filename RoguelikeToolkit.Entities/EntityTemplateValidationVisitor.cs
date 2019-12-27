using System;
using System.Collections.Generic;
using System.Linq;

namespace RoguelikeToolkit.Entities
{
    public class EntityTemplateValidationVisitor : EntityTemplateBaseVisitor<bool>
    {
        public bool HasComponentsField { get; set; }
        public bool HasInheritsField { get; set; }
        public bool HasIdentifierField { get; set; }

        private readonly Stack<string> _embeddedObjectContext = new Stack<string>();
        private readonly Dictionary<string, int> _doubleFieldCount = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);

        public void Reset()
        {
            HasComponentsField = false;
            HasIdentifierField = false;
            HasInheritsField = false;
            _doubleFieldCount.Clear();
        }

        public IEnumerable<string> DuplicateFields => _doubleFieldCount.Where(x => x.Value > 1).Select(x => x.Key.Substring(x.Key.LastIndexOf('.') + 1));

        public override bool VisitObject(EntityTemplateParser.ObjectContext context)
        {
            _embeddedObjectContext.Push($"obj{context.Depth()}");
            try
            {
                return base.VisitObject(context);
            }
            finally
            {
                _embeddedObjectContext.Pop();
            }
        }

        public override bool VisitRegularField(EntityTemplateParser.RegularFieldContext context)
        {
            _doubleFieldCount.AddOrSet($"{string.Join(".",_embeddedObjectContext)}.{context.key.Text.Replace("\"",string.Empty)}", existing => ++existing);
            return base.VisitRegularField(context);
        }

        public override bool VisitIdentifierField(EntityTemplateParser.IdentifierFieldContext context)
        {
            _doubleFieldCount.AddOrSet($"{string.Join(".",_embeddedObjectContext)}.{context.key.Text.Replace("\"",string.Empty)}", existing => ++existing);
            HasIdentifierField = true;
            return base.VisitIdentifierField(context);
        }

        public override bool VisitInheritsField(EntityTemplateParser.InheritsFieldContext context)
        {
            _doubleFieldCount.AddOrSet($"{string.Join(".",_embeddedObjectContext)}.{context.key.Text.Replace("\"",string.Empty)}", existing => ++existing);
            HasInheritsField = true;
            return base.VisitInheritsField(context);
        }

        public override bool VisitComponentsField(EntityTemplateParser.ComponentsFieldContext context)
        {
            _doubleFieldCount.AddOrSet($"{string.Join(".",_embeddedObjectContext)}.{context.key.Text.Replace("\"",string.Empty)}", existing => ++existing);
            HasComponentsField = true;
            return base.VisitComponentsField(context);
        }

        protected override bool AggregateResult(bool aggregate, bool nextResult) => 
            HasComponentsField && HasIdentifierField && HasInheritsField && _doubleFieldCount.All(x => x.Value <= 1);
    }
}
