namespace RoguelikeToolkit.Entities
{ 
    public struct MetadataComponent : IValueComponent<string[]>
    {
        public string[] Value { get; set; }
    }
}
