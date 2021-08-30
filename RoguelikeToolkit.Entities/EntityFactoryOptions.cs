namespace RoguelikeToolkit.Entities
{
    public class EntityFactoryOptions
    {
        public bool IgnoreMissingFields { get; set; }

        public static EntityFactoryOptions Default { get; } = new EntityFactoryOptions
        {
            IgnoreMissingFields = true
        };
    }
}
