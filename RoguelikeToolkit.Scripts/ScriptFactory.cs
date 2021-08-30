using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Linq;
using System.Reflection;

namespace RoguelikeToolkit.Scripts
{
    public static class ScriptFactory
    {
        public static Script<object> CreateCompiled<TParams>(string actionScript, params Assembly[] referenceAssemblies)
        {
            var allReferenceAssemblies = referenceAssemblies.Concat(Constants.AssembliesToReference).ToList();
            var diceAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.FullName.Contains("RoguelikeToolkit.Dice"));
            if (diceAssembly != null)
            {
                allReferenceAssemblies.Add(diceAssembly);
            }

            var allNamespaces = allReferenceAssemblies.SelectMany(a => a.GetTypes().Select(t => t.Namespace)).Where(n => n != null).Distinct().ToArray();
#if RELEASE
            var _compiledScript = CSharpScript.Create(actionScript,
                ScriptOptions.Default
                    .WithReferences(allReferenceAssemblies)
                    .WithImports(allNamespaces)
                    .WithOptimizationLevel(OptimizationLevel.Release),
            globalsType: typeof(TParams));
#else
            var _compiledScript = CSharpScript.Create(actionScript,
                ScriptOptions.Default
                    .WithReferences(allReferenceAssemblies)
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
