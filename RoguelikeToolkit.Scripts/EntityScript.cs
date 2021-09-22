using System;
using DefaultEcs;

namespace RoguelikeToolkit.Scripts
{
    public class EntityScript
    {
        private readonly Script _script;

        public EntityScript(string script, string targetInstanceName = null) => 
            _script = new Script(script, targetInstanceName ?? "component");

        public bool TryExecuteOn<TComponent>(in Entity entity)
        {
            if(!entity.Has<TComponent>())
                return false;

            ref var component = ref entity.Get<TComponent>();
            _script.ExecuteOn(ref component);

            return true;
        }
    }
}
