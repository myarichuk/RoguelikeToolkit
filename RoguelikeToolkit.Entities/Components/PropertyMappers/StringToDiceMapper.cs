﻿using System;
using System.Collections.Generic;
using RoguelikeToolkit.DiceExpression;
using RoguelikeToolkit.Entities.Components.TypeMappers;

namespace RoguelikeToolkit.Entities.Components.PropertyMappers
{

    public class StringToDiceMapper : IPropertyMapper
    {
        public int Priority => 10;

        public bool CanMap(Type destType, object value)
        {
            if (destType is null)
            {
                throw new ArgumentNullException(nameof(destType));
            }

            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return destType == typeof(Dice) && value is string;
        }

        public object Map(IReadOnlyList<IPropertyMapper> propertyMappers, Type destType, object value, Type[] componentTypes) =>
            Dice.Parse((string)value);
    }
}