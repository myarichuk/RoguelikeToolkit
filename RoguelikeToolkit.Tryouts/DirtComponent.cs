using System;
using System.Collections.Generic;
using System.Text;
using RoguelikeToolkit.Entities;

namespace RoguelikeToolkit.Tryouts
{
    public class DirtComponent : IValueComponent<double>
    {
        public double Value { get; set; }
    }
}
