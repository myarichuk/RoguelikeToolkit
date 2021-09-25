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