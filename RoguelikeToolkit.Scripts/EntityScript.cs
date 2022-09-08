using System;
using System.Runtime.CompilerServices;
using DefaultEcs;

namespace RoguelikeToolkit.Scripts
{
    public class EntityScript
    {
        private readonly Script _script;

        public EntityScript(string script, params Type[] referenceTypes) => 
            _script = new Script(script, referenceTypes);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExecuteOn(ref Entity entity, params (string instanceName, dynamic value)[] @params) => 
            _script.ExecuteOn(ref entity, "entity", @params);
    }
}
