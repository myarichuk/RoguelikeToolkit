using RoguelikeToolkit.DiceExpression;
using RoguelikeToolkit.Scripts;

namespace RoguelikeToolkit.Entities.Tests
{
    [Component(Name = "KickAbility")]
    public struct KickAbility
    {
        public Dice Strength {  get; set; }

        public EntityInteractionScript Effect {  get; set; }
    }
}
