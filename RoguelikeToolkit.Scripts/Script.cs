using System;
using System.Collections.Generic;
using System.Text;
using DefaultEcs;
using Jint;

namespace RoguelikeToolkit.Scripts
{

    public class Script
    {
        private readonly Engine _engine = EngineFactory.Create();

        private const string DefaultTargetInstanceName = "target";
        private readonly string _script;
        private readonly string _targetInstanceName;

        public Script(string script, string targetInstanceName = null)
        {
            _script = script;
            _targetInstanceName = targetInstanceName ?? DefaultTargetInstanceName;
        }

        public void ExecuteOn<TTarget>(ref TTarget target, string overrideTargetInstanceName = null)
        {
            _engine.SetValue(overrideTargetInstanceName ?? _targetInstanceName, target)
                   .Execute(_script);
            
            var result = (TTarget)_engine.GetValue(overrideTargetInstanceName ?? _targetInstanceName).ToObject();
            target = result;
        }       
    }
}
