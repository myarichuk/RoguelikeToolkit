using DefaultEcs;
using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RoguelikeToolkit.Scripts
{

    public class EntityInteractionScript : EntityScript<EntityInteractionParam>
    {
        private readonly static ObjectPool<EntityInteractionParam> ParamPool = new DefaultObjectPool<EntityInteractionParam>(new DefaultPooledObjectPolicy<EntityInteractionParam>());

        public EntityInteractionScript(string actionScript, params Assembly[] referenceAssemblies) : base(actionScript, referenceAssemblies)
        {
        }

        public Task RunAsyncOn(in Entity source, in Entity target, CancellationToken? ct = null)
        {
            EntityInteractionParam param = null;
            try
            {
                param = ParamPool.Get();
                param.source = source;
                param.target = target;

                return _compiledScript.RunAsync(param, ct ?? CancellationToken.None);
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
