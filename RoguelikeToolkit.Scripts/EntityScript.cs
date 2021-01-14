using DefaultEcs;
using Microsoft.CodeAnalysis.Scripting;
using System.Threading;
using System.Threading.Tasks;

namespace RoguelikeToolkit.Scripts
{
    public class EntityScript
    {

        private readonly Script<object> _compiledScript;

        public EntityScript(string actionScript) =>
            _compiledScript = ScriptFactory.CreateCompiled<EntityParam>(actionScript);

        public Task RunAsyncOn(Entity entity, CancellationToken? ct = null) =>
            //TODO: object pool creation of ActionParams to conserve GC (no need to new-it-up it each time...)
            _compiledScript.RunAsync(new EntityParam
            {
                entity = entity
            }, ct ?? CancellationToken.None);
    }
}
