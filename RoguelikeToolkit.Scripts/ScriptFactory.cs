using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.Linq;

namespace RoguelikeToolkit.Scripts
{
    public static class ScriptFactory
    {
        public static Script<object> CreateCompiled<TParams>(string actionScript)
        {
            var allNamespaces = Constants.AssembliesToReference.SelectMany(a => a.GetTypes().Select(t => t.Namespace)).Where(n => n != null).Distinct().ToArray();
#if RELEASE
            var _compiledScript = CSharpScript.Create(actionScript,
                ScriptOptions.Default
                    .WithReferences(Constants.AssembliesToReference)
                    .WithImports(allNamespaces)
                    .WithOptimizationLevel(OptimizationLevel.Release),
            globalsType: typeof(TParams));
#else
            var _compiledScript = CSharpScript.Create(actionScript,
                ScriptOptions.Default
                    .WithReferences(Constants.AssembliesToReference)
                    .WithImports(allNamespaces)
                    .WithEmitDebugInformation(true)
                    .WithOptimizationLevel(OptimizationLevel.Debug),
            globalsType: typeof(TParams));
#endif
            _compiledScript.Compile();

            return _compiledScript;
        }
    }
}
