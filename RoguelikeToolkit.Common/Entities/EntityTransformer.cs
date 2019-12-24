using DefaultEcs;

namespace RoguelikeToolkit.Common.Entities
{
    //gives an opportunity to transform an entity after entity factory creates it
    public interface IEntityTransformer
    {
        bool ShouldTransform(World world, ref Entity entity);
        void Tranform(World world, ref Entity entity);
    }
}
