using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using DefaultEcs;
using Jint;

namespace RoguelikeToolkit.Scripts
{

    public class Script
    {
        private readonly Engine _engine;
        
        public const string SingleTargetInstanceName = "target";

        public const string TwoTargetSourceInstanceName = "source";
        public const string TwoTargetTargetInstanceName = "target";


        private readonly string _script;

        public Script(string script, params Type[] referenceTypes)
        {
            _script = script;
            _engine = EngineFactory.Create(referenceTypes);
        }    

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExecuteOn<TTarget>(ref TTarget target, params (string instanceName, dynamic value)[] @params) =>
            ExecuteOn(ref target, SingleTargetInstanceName, @params);

        public void ExecuteOn<TTarget>(ref TTarget target, string instanceName, params (string instanceName, dynamic value)[] @params)
        {
            foreach (var param in @params)
                _engine.SetValue(param.instanceName, param.value);

            _engine.SetValue(instanceName, target)
                   .Execute(_script);
            
            var result = (TTarget)_engine.GetValue(instanceName).ToObject();
            target = result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExecuteOn<TSource, TTarget>(ref TSource source, ref TTarget target, params (string instanceName, dynamic value)[] @params) =>
            ExecuteOn(ref source, TwoTargetSourceInstanceName, ref target, TwoTargetTargetInstanceName, @params);

        public void ExecuteOn<TSource, TTarget>(ref TSource source, string sourceInstanceName, ref TTarget target, string targetInstanceName, params (string instanceName, dynamic value)[] @params)
        {
            foreach (var param in @params)
                _engine.SetValue(param.instanceName, param.value);

            _engine.SetValue(targetInstanceName, target)
                   .SetValue(sourceInstanceName, source)
                   .Execute(_script);

            var changedTarget = (TTarget)_engine.GetValue(targetInstanceName).ToObject();
            target = changedTarget;

            var changedSource = (TSource)_engine.GetValue(sourceInstanceName).ToObject();
            source = changedSource;
        }
    }
}
