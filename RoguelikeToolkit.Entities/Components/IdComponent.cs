namespace RoguelikeToolkit.Entities.Components
{
	public record struct IdComponent : IValueComponent<string>
	{
		public string Value { get; set; }
	}
}
