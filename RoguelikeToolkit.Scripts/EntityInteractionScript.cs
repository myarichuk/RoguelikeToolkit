using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DefaultEcs;
using Microsoft.Extensions.ObjectPool;

namespace RoguelikeToolkit.Scripts
{

    public class EntityInteractionScript : EntityScript<EntityInteractionParam>
    {
        private static readonly ObjectPool<EntityInteractionParam> ParamPool = new DefaultObjectPool<EntityInteractionParam>(new DefaultPooledObjectPolicy<EntityInteractionParam>());

        public EntityInteractionScript(string actionScript, params Assembly[] referenceAssemblies) : base(actionScript, referenceAssemblies)
        {
        }

        public ValueTask RunAsyncOn(in Entity source, in Entity target, CancellationToken ct = default)
        {
            EntityInteractionParam param = null;
            try
            {
                param = ParamPool.Get();
                param.source = source;
                param.target = target;
                return new ValueTask(_compiledScript.RunAsync(param, ct));
            }
            finally
            {
                if (param != null)
                {
                    param.source = default;
                    param.target = default;
                    ParamPool.Return(param);
                }
            }
        }
    }
}
