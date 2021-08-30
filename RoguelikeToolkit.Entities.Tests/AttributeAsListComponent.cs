using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoguelikeToolkit.Entities.Tests
{
    public class AttributeAsListComponent : IValueComponent<List<double>>
    {
        public List<double> Value {  get; set; }
    }

    public class AttributeAsListInterfaceComponent : IValueComponent<IEnumerable<double>>
    {
        public IEnumerable<double> Value { get; set; }
    }

    public class AttributeAsHashSetComponent : IValueComponent<HashSet<KnownColor>>
    {
        public HashSet<KnownColor> Value { get; set; }
    }
}
