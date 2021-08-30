using Antlr4.Runtime;
using System.Runtime.CompilerServices;

// ReSharper disable once IdentifierTypo
namespace RoguelikeToolkit.DiceExpression
{
    public partial class Dice
    {
        #region Static Parser
        private static readonly DiceLexer Lexer;
        private static readonly DiceParser Parser;

        static Dice()
        {
            Lexer = new DiceLexer(new AntlrInputStream(string.Empty));
            Parser = new DiceParser(new CommonTokenStream(Lexer));
        }
        #endregion

        private readonly DiceParser.RootContext _diceAst;

        internal Dice(DiceParser.RootContext diceAst) => _diceAst = diceAst;

        //relevant for resolving as a component of RoguelikeToolkit.Entities
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Dice(string diceExpression) => Parse(diceExpression);

        public static Dice Parse(string diceExpression)
        {
            Lexer.SetInputStream(new AntlrInputStream(diceExpression));
            Parser.SetInputStream(new CommonTokenStream(Lexer));

            var diceAst = Parser.root();
            return new Dice(diceAst);
        }

        public int Roll()
        {
            return new DiceEvaluator().Visit(_diceAst);
        }

    }
}
