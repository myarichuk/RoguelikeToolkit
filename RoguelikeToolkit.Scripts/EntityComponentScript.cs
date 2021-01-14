using DefaultEcs;
using Microsoft.CodeAnalysis.Scripting;
using System.Threading;
using System.Threading.Tasks;

namespace RoguelikeToolkit.Scripts
{
    public class EntityComponentScript
    {
        private readonly Script<object> _compiledScript;
        public EntityComponentScript(string actionScript) =>
            _compiledScript = ScriptFactory.CreateCompiled<ComponentParam>(actionScript);

        public Task RunAsyncOn<TComponent>(in Entity entity, CancellationToken? ct = null)
        {
            var component = entity.Get<TComponent>();
            //TODO: object pool creation  of ComponentParam to conserve GC (no need to new-it-up it each time...)
            return _compiledScript.RunAsync(new ComponentParam
            {
                component = component
            }, ct ?? CancellationToken.None);
        }
    }
}
