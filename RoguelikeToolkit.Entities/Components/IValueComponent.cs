﻿namespace RoguelikeToolkit.Entities
{
    public interface IValueComponent<TValue>
    {
        TValue Value { get; set; }
    }
}
