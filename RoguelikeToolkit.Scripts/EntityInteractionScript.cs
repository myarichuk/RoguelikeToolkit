using System;
using System.Runtime.CompilerServices;
using DefaultEcs;

namespace RoguelikeToolkit.Scripts
{
    public class EntityInteractionScript
    {
        private readonly Script _script;

        public EntityInteractionScript(string script, params Type[] referenceTypes) =>
            _script = new Script(script, referenceTypes);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExecuteOn(ref Entity source, ref Entity target, params (string instanceName, dynamic value)[] @params) =>
            _script.ExecuteOn(ref source, "source", ref target, "target", @params);
    }
}


//using DefaultEcs;

//namespace RoguelikeToolkit.Scripts
//{
//    public class EntityInteractionScript
//    {
//        private readonly InteractionScript _script;

//        public EntityInteractionScript(string script, string sourceInstanceName = null, string targetInstanceName = null) =>
//            _script = new InteractionScript(script, sourceInstanceName, targetInstanceName);

//        public bool TryExecuteOn<TSourceComponent, TTargetComponent>(in Entity sourceEntity, in Entity targetEntity)
//        {
//            if (!sourceEntity.Has<TSourceComponent>() ||
//                !targetEntity.Has<TTargetComponent>())
//                    return false;

//            ref var source = ref sourceEntity.Get<TSourceComponent>();
//            ref var target = ref targetEntity.Get<TTargetComponent>();

//            _script.ExecuteOn(ref source, ref target);
//            return true;
//        }
//    }
//}