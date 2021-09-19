using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DefaultEcs;
using Microsoft.Extensions.ObjectPool;

namespace RoguelikeToolkit.Scripts
{
    public class EntityComponentScript : EntityScript<ComponentParam>
    {
        private static readonly ObjectPool<ComponentParam> ParamPool = new DefaultObjectPool<ComponentParam>(new DefaultPooledObjectPolicy<ComponentParam>());

        public EntityComponentScript(string actionScript, params Assembly[] referenceAssemblies) : base(actionScript, referenceAssemblies)
        {
        }

        public async Task RunAsyncOn<TComponent>(Entity entity, CancellationToken ct = default)
        {
            if (!entity.Has<TComponent>())
            {
                return;
            }

            ComponentParam param = null;
            try
            {
                var component = entity.Get<TComponent>();
                param = ParamPool.Get();
                param.component = component;

                await _compiledScript.RunAsync(param, ct);
                entity.Set((TComponent)param.component);
            }
            finally
            {
                if (param != null)
                {
                    param.component = default;
                    ParamPool.Return(param);
                }
            }
        }
    }
}
