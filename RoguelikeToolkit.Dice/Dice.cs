using System.Data;
using Antlr4.Runtime;

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
            Lexer= new DiceLexer(new AntlrInputStream(string.Empty));
            Parser = new DiceParser(new CommonTokenStream(Lexer));
        }
        #endregion

        private readonly DiceParser.RootContext _diceAst;

        internal Dice(DiceParser.RootContext diceAst) => _diceAst = diceAst;

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
