using DefaultEcs;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.ObjectPool;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RoguelikeToolkit.Scripts
{
    public class EntityScript<TScriptParam>
        where TScriptParam : class
    {
        protected readonly Script<object> _compiledScript;

        public EntityScript(string actionScript) =>
            _compiledScript = ScriptFactory.CreateCompiled<TScriptParam>(actionScript);
        public Task RunAsyncOn(in Entity entity, Func<Entity, TScriptParam> paramFactory, CancellationToken? ct = null) =>
            _compiledScript.RunAsync(paramFactory(entity), ct ?? CancellationToken.None);
    }

    public class EntityScript : EntityScript<EntityParam>
    {
        private readonly static ObjectPool<EntityParam> ParamPool = new DefaultObjectPool<EntityParam>(new DefaultPooledObjectPolicy<EntityParam>());

        public EntityScript(string actionScript) : base(actionScript)
        {
        }

        public Task RunAsyncOn(Entity entity, CancellationToken? ct = null)
        {
            EntityParam @param = null;
            try
            {
                param = ParamPool.Get();
                param.entity = entity;

                return _compiledScript.RunAsync(new EntityParam
                {
                    entity = entity
                }, ct ?? CancellationToken.None);
            }
            finally
            {
                if (param != null)
                {
                    param.entity = default;
                    ParamPool.Return(param);
                }
            }
        }
    }
}
