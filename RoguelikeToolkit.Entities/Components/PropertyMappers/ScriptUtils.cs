using System.IO;

namespace RoguelikeToolkit.Entities.Components.PropertyMappers
{
    public static class ScriptUtils
    {
        private const string ScriptPrefix = "file(";
        private const string ScriptSuffix = ")";

        public static bool IsScriptPath(string templateField)
        {
            if(templateField.StartsWith(" ") || templateField.EndsWith(" "))
                templateField = templateField.Trim();

            if(templateField.StartsWith(ScriptPrefix, System.StringComparison.InvariantCultureIgnoreCase) &&
               templateField.EndsWith(ScriptSuffix))
            {
                return true;
            }

            return false;
        }

        public static bool TryGetScript(string templateField, out string script)
        {
            script = string.Empty;

            if (!IsScriptPath(templateField))
                return false;

            var path = templateField.Substring(ScriptPrefix.Length, templateField.Length - ScriptPrefix.Length - ScriptSuffix.Length);
           
            if (path.Length == 0 || !File.Exists(path))
                return false;

            script = File.ReadAllText(path);
            return true;
        }
    }
}