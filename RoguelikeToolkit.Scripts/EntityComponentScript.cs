using System;
using DefaultEcs;

namespace RoguelikeToolkit.Scripts
{
    public class EntityComponentScript
    {
        private readonly Script _script;

        public EntityComponentScript(string script, params Type[] referenceTypes) =>
            _script = new Script(script, referenceTypes);

        public bool TryExecuteOn<TComponent>(ref Entity entity, params (string instanceName, dynamic value)[] @params)
        {
            if(!entity.Has<TComponent>())
                return false;

            ref var component = ref entity.Get<TComponent>();

            _script.ExecuteOn(ref component, "component", @params);

            entity.Set(component);

            return true;
        }

        public bool TryExecuteOn<TSourceComponent, TTargetComponent>(ref Entity source, ref Entity target, params (string instanceName, dynamic value)[] @params)
        {
            if(!source.Has<TSourceComponent>() || !target.Has<TTargetComponent>())
                return false;

            ref var sourceComponent = ref source.Get<TSourceComponent>();
            ref var targetComponent = ref target.Get<TTargetComponent>();

            _script.ExecuteOn(ref sourceComponent, ref targetComponent, @params);

            source.Set(sourceComponent);
            target.Set(targetComponent);

            return true;
        }
    }
}
