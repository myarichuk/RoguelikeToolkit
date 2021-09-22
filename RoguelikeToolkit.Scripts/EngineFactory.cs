using Jint;

namespace RoguelikeToolkit.Scripts
{
    internal static class EngineFactory
    {
        public static Engine Create() => new Engine((eng, opt) =>
        {            
            opt.LimitRecursion(1024)
               .LimitMemory(1024 * 1024)
               .MaxStatements(500)
#if DEBUG
               .DebugMode()
#endif
               .AllowClr();
        });
    }
}
