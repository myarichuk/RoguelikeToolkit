using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DefaultEcs;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.ObjectPool;

namespace RoguelikeToolkit.Scripts
{
    public class EntityScript<TScriptParam>
        where TScriptParam : class
    {
        protected readonly Script<object> _compiledScript;

        public EntityScript(string actionScript, params Assembly[] referenceAssemblies) =>
            _compiledScript = ScriptFactory.CreateCompiled<TScriptParam>(actionScript, referenceAssemblies);

        public Task RunAsyncOn(in Entity entity, Func<Entity, TScriptParam> paramFactory, CancellationToken ct = default) =>
            _compiledScript.RunAsync(paramFactory(entity), ct);
    }

    public class EntityScript : EntityScript<EntityParam>
    {
        private static readonly ObjectPool<EntityParam> ParamPool = new DefaultObjectPool<EntityParam>(new DefaultPooledObjectPolicy<EntityParam>());

        public EntityScript(string actionScript, params Assembly[] referenceAssemblies) : base(actionScript, referenceAssemblies)
        {
        }

        public Task RunAsyncOn(in Entity entity, CancellationToken ct = default)
        {
            EntityParam @param = null;
            try
            {
                param = ParamPool.Get();
                param.entity = entity;

                return _compiledScript.RunAsync(param, ct);
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
