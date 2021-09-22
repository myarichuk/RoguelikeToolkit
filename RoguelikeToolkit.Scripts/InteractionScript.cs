using Jint;

namespace RoguelikeToolkit.Scripts
{
    public class InteractionScript
    {
        private readonly Engine _engine = EngineFactory.Create();

        private const string DefaultSourceInstanceName = "source";
        private const string DefaultTargetInstanceName = "target";
        private readonly string _script;
        private readonly string _sourceInstanceName;
        private readonly string _targetInstanceName;

        public InteractionScript(string script, string sourceInstanceName = null, string targetInstanceName = null)
        {
            _script = script;
            _targetInstanceName = targetInstanceName ?? DefaultTargetInstanceName;
            _sourceInstanceName = sourceInstanceName ?? DefaultSourceInstanceName;
        }

        public void ExecuteOn<TSource, TTarget>(ref TSource source, ref TTarget target)
        {
            _engine.SetValue(_targetInstanceName, target)
                   .SetValue(_sourceInstanceName, source)
                   .Execute(_script);

            var changedTarget = (TTarget)_engine.GetValue(_targetInstanceName).ToObject();
            target = changedTarget;

            var changedSource = (TSource)_engine.GetValue(_sourceInstanceName).ToObject();
            source = changedSource;
        }
    }
}
