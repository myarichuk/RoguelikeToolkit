﻿using System;
using System.Collections.Generic;
using System.Text;
using RoguelikeToolkit.Entities;

namespace RoguelikeToolkit.Tryouts
{
    [Component(Name = "Attributes")]
    public class Attributes
    {
        public int Strength { get; set; }
        public int Agility;
    }
}
