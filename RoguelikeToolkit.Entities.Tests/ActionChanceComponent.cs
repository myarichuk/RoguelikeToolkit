using RoguelikeToolkit.DiceExpression;
using RoguelikeToolkit.Scripts;

namespace RoguelikeToolkit.Entities.Tests
{
    public class ActionChanceComponent
    {
        public Dice Dice { get; set; }

        public double Result { get; set; }

        public EntityComponentScript ActionScript { get; set; }
    }
}
