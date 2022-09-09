namespace RoguelikeToolkit.Entities.Components
{
    public interface IValueComponent<TValue>
    {
        TValue Value { get; set; }
    }
}
