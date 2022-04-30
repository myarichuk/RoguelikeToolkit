using RoguelikeToolkit.DiceExpression;

namespace RoguelikeToolkit.Tryouts
{
    public class FoobarComponent
    {
#pragma warning disable S1104 // Fields should not have public accessibility
        public Dice Dice1;
#pragma warning restore S1104 // Fields should not have public accessibility
#pragma warning disable S1104 // Fields should not have public accessibility
        public Dice Dice2;
#pragma warning restore S1104 // Fields should not have public accessibility

#pragma warning disable S1104 // Fields should not have public accessibility
        public int RollResult;
#pragma warning restore S1104 // Fields should not have public accessibility
    }
}
