namespace RoguelikeToolkit.Common
{
    public interface IValueComponent<TValue>
    {
        TValue Value { get; set; }
    }
}
