using DefaultEcs;
using DefaultEcs.System;
using DefaultEcs.Threading;
using RoguelikeToolkit.Common;

namespace RoguelikeToolkit.Actor
{
    public abstract class ActorSystem<TGameState> : AEntitySystem<TGameState> where TGameState : IGameState
    {
        protected ActorSystem(World world, IParallelRunner runner) 
            : base(world.GetEntities()
                        .With<BodyPart>()
                        .With<Position>()
                        .AsSet(), runner)
        {
        }

        protected abstract void UpdateActor(TGameState gameState, in Entity entity);
        protected override void Update(TGameState gameState, in Entity entity) => UpdateActor(gameState, entity);
    }
}
