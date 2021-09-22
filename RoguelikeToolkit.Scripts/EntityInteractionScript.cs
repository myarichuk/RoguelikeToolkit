using DefaultEcs;

namespace RoguelikeToolkit.Scripts
{
    public class EntityInteractionScript
    {
        private readonly InteractionScript _script;

        public EntityInteractionScript(string script, string sourceInstanceName = null, string targetInstanceName = null) =>
            _script = new InteractionScript(script, sourceInstanceName, targetInstanceName);

        public bool TryExecuteOn<TSourceComponent, TTargetComponent>(in Entity sourceEntity, in Entity targetEntity)
        {
            if (!sourceEntity.Has<TSourceComponent>() ||
                !targetEntity.Has<TTargetComponent>())
                    return false;

            ref var source = ref sourceEntity.Get<TSourceComponent>();
            ref var target = ref targetEntity.Get<TTargetComponent>();

            _script.ExecuteOn(ref source, ref target);
            return true;
        }
    }
}
