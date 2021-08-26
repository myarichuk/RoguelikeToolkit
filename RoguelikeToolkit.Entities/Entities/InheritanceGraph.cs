using System;
using System.Collections.Generic;
using System.Text;

namespace RoguelikeToolkit.Entities
{
    public class InheritanceGraph
    {
        public struct Node
        {
            public EntityTemplate Template;
            public List<Node> Adjacent;
        }

        private readonly List<Node> _roots = new();

        public InheritanceGraph(IEnumerable<EntityTemplate> templates)
        {
            BuildRootList(templates);

        }



        #region Helpers

        private void BuildRootList(IEnumerable<EntityTemplate> templates)
        {
            foreach (var template in templates)
            {
                if (template.Inherits.Count == 0)
                    _roots.Add(new Node { Template = template, Adjacent = new List<Node>() });
            }
        }

        #endregion
    }
}
