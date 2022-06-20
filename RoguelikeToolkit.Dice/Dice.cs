﻿using System.Data;
using System.Runtime.CompilerServices;
using Antlr4.Runtime;

// ReSharper disable once IdentifierTypo
namespace RoguelikeToolkit.DiceExpression
{
	public partial class Dice
	{
		#region Static Parser

		private static readonly DiceLexer Lexer = new(new AntlrInputStream(string.Empty));
		private static readonly DiceParser Parser = new(new CommonTokenStream(Lexer));

		#endregion

		private readonly DiceParser.RootContext _diceAst;

		internal Dice(DiceParser.RootContext diceAst) => _diceAst = diceAst;

		public static implicit operator string(Dice dice) => dice._diceAst.ToStringTree();

        public override string ToString() => _diceAst?.ToStringTree() ?? string.Empty;

        //relevant for resolving as a component of RoguelikeToolkit.Entities
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator Dice(string diceExpression) => Parse(diceExpression);

		[MethodImpl(MethodImplOptions.Synchronized)]
		public static Dice Parse(string diceExpression, bool throwOnParsingError = false)
		{
			Lexer.SetInputStream(new AntlrInputStream(diceExpression));
			Parser.SetInputStream(new CommonTokenStream(Lexer));

			if (throwOnParsingError)
			{
				var listener = new DiagnosticErrorListener();
				Parser.AddErrorListener(listener);
			}

			var diceAst = Parser.root();

			if (throwOnParsingError && Parser.NumberOfSyntaxErrors > 0)
			{
				throw new SyntaxErrorException($"Failed to parse expression '{diceExpression}'");
			}

			return new Dice(diceAst);
		}

        public int Roll() => new DiceEvaluator().Visit(_diceAst);

    }
}
