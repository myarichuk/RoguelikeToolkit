using RoguelikeToolkit.Scripts;

namespace RoguelikeToolkit.Entities.Tests
{
    public struct ActionScriptComponent : IValueComponent<EntityScript>
    {
        public EntityScript Value {  get; set; }
    }
}
