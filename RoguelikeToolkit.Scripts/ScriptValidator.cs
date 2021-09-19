using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Scripting;

namespace RoguelikeToolkit.Scripts
{
    public class ScriptValidator
    {
        public bool IsValid(Script script)
        {
            var compilation = script.GetCompilation();

            foreach(var st in compilation.SyntaxTrees)
                if(!ValidateSyntaxTree(compilation, st))
                    return false;
            return true;
        }

        private bool ValidateSyntaxTree(Compilation compilation, SyntaxTree syntaxTree)
        {
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var syntaxRootNode = syntaxTree.GetRoot() as CompilationUnitSyntax;
            foreach (UsingDirectiveSyntax usingDirective in syntaxRootNode.Usings)
            {
                var symbol = semanticModel.GetSymbolInfo(usingDirective.Name).Symbol;
                var name = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                if (name.StartsWith("global::"))
                    name = name.Replace("global::", string.Empty);

                if (IsIllegalAssemblyName(name))
                    throw new InvalidOperationException($"The scripting environment is a sandbox, using assembly '{name}' is forbidden");

            }
            return true;
        }

        private static bool IsIllegalAssemblyName(string assemblyName) =>
                    assemblyName switch
                    {
                        "System.Console" => true,
                        "System.Diagnostics.Debug" => true,
                        "System.IO" => true,
                        "System.IO.FileSystem" => true,
                        "System.Net.Http" => true,
                        "System.Net.Http.Json" => true,
                        "System.Web" => true,
                        "System.Web.HttpUtility" => true,
                        "System.IO.FileSystem.Primitives" => true,
                        "System.Reflection" => true,
                        "System.Reflection.Extensions" => true,
                        "System.Runtime" => true,
                        "System.Runtime.Extensions" => true,
                        "System.Runtime.InteropServices" => true,
                        "System.Diagnostics.Tracing" => true,
                        "System.Runtime.CompilerServices.Unsafe" => true,
                        "System.Memory" => true,
                        "System.Reflection.Emit.ILGeneration" => true,
                        "System.Diagnostics.Tools" => true,
                        "System.Reflection.Metadata" => true,
                        "System.IO.Compression" => true,
                        "System.IO.MemoryMappedFiles" => true,
                        "System.Diagnostics.FileVersionInfo" => true,
                        "Microsoft.Win32.Registry" => true,
                        "System.Security.AccessControl" => true,
                        _ => false,
                    };
    }
}
