namespace RoguelikeToolkit.Entities
{
    public class EntityFactoryOptions
    {
        public bool IgnoreMissingFields { get; set; }

        public bool AutoIncludeIdComponent { get; set; }

        public static EntityFactoryOptions Default { get; } = new EntityFactoryOptions
        {
            IgnoreMissingFields = true,
            AutoIncludeIdComponent = true
        };
    }
}
