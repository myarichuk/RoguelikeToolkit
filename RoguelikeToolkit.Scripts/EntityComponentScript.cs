using DefaultEcs;
using Microsoft.Extensions.ObjectPool;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace RoguelikeToolkit.Scripts
{
    public class EntityComponentScript : EntityScript<ComponentParam>
    {
        private readonly static ObjectPool<ComponentParam> ParamPool = new DefaultObjectPool<ComponentParam>(new DefaultPooledObjectPolicy<ComponentParam>());

        public EntityComponentScript(string actionScript, params Assembly[] referenceAssemblies) : base(actionScript, referenceAssemblies)
        {
        }

        public Task RunAsyncOn<TComponent>(in Entity entity, CancellationToken? ct = null)
        {
            if (!entity.Has<TComponent>())
                return Task.CompletedTask;

            ComponentParam param = null;
            try
            {
                var component = entity.Get<TComponent>();
                param = ParamPool.Get();
                param.component = component;

                return _compiledScript.RunAsync(param, ct ?? CancellationToken.None);
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
