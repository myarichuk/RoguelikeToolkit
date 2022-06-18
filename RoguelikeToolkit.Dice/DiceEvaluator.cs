using Antlr4.Runtime.Misc;
using System;
using System.Linq;

namespace RoguelikeToolkit.DiceExpression
{
	public class DiceEvaluator : DiceBaseVisitor<int>
	{
		protected override int AggregateResult(int aggregate, int nextResult) => aggregate + nextResult;

		public override int VisitDiceAddSubstractExpression(DiceParser.DiceAddSubstractExpressionContext context)
		{
			var left = context.left.Accept(this);
			var right = context.right.Accept(this);

			switch (context.op.Type)
			{
				case DiceLexer.PLUS:
					return left + right;
				case DiceLexer.MINUS:
					return left - right;
				default:
					throw new ArgumentException(
						$"Unexpected operator token '{context.op.Text}' (in this context expected either '+' or '-')");
			}
		}

		public override int VisitDiceParenthesisExpression(DiceParser.DiceParenthesisExpressionContext context) => context.dice().Accept(this);

		public override int VisitDice10KeepExpression(DiceParser.Dice10KeepExpressionContext context)
		{
			var rolls = Dice.Roll(int.Parse(context.numOfDice.Text), 10).OrderByDescending(x => x);
			return rolls.Take(int.Parse(context.keepNum.Text)).Sum();
		}

		public override int VisitDiceMultiplyDivideExpression(DiceParser.DiceMultiplyDivideExpressionContext context)
		{
			var left = context.left.Accept(this);
			var right = context.right.Accept(this);

			switch (context.op.Type)
			{
				case DiceLexer.MULTIPLY:
					return left * right;
				case DiceLexer.DIVIDE:
					return left / right;
				default:
					throw new ArgumentException(
						$"Unexpected operator token '{context.op.Text}' (in this context expected either '+' or '-')");
			}
		}


		public override int VisitDiceConstantException(DiceParser.DiceConstantExceptionContext context) => int.Parse(context.NUMBER().GetText());

		public override int VisitDice100Expression(DiceParser.Dice100ExpressionContext context) => Dice.Roll100();

		public override int VisitOneDiceExpression(DiceParser.OneDiceExpressionContext context)
		{
			var numOfDice = 1;
			var sides = int.Parse(context.sides.Text);

			if (context.keepNum == null)
			{
				return Dice.RollAndSum(numOfDice, sides);
			}

			//we need to keep the highest 'keepNum' for summary
			var rolls = Dice.Roll(numOfDice, sides).OrderByDescending(x => x);
			return rolls.Take(int.Parse(context.keepNum.Text)).Sum();
		}

		public override int VisitDiceExpression(DiceParser.DiceExpressionContext context)
		{
			var numOfDice = int.Parse(context.numOfDice.Text);
			var sides = int.Parse(context.sides.Text);

			if (context.keepNum == null)
			{
				return Dice.RollAndSum(numOfDice, sides);
			}

			//we need to keep the highest 'keepNum' for summary
			var rolls = Dice.Roll(numOfDice, sides).OrderByDescending(x => x);
			return rolls.Take(int.Parse(context.keepNum.Text)).Sum();

		}

		public override int VisitExplodingConstantDiceExpression([NotNull] DiceParser.ExplodingConstantDiceExpressionContext context)
		{
			var diceContext = ((DiceParser.DiceExpressionContext)context.d);

			var numOfDice = int.Parse(diceContext.numOfDice.Text);
			var sides = int.Parse(diceContext.sides.Text);


			return Dice.RollExploding(numOfDice, sides);
		}

		public override int VisitExplodingConditionalDiceExpression([NotNull] DiceParser.ExplodingConditionalDiceExpressionContext context)
		{
			var diceContext = ((DiceParser.DiceExpressionContext)context.d);

			var numOfDice = int.Parse(diceContext.numOfDice.Text);
			var sides = int.Parse(diceContext.sides.Text);
			var threshold = int.Parse(context.NUMBER().GetText());
			var op = context.op.Text;

			bool Condition(int result)
			{
				return op switch
				{
					">" => result >= threshold,
					"<" => result <= threshold,
					_ => throw new NotSupportedException($"Not supported operator ({op})")
				};
			}

			return Dice.RollExploding(numOfDice, sides, Condition);
		}

		public override int VisitExplodingThresholdDiceExpression([NotNull] DiceParser.ExplodingThresholdDiceExpressionContext context)
		{
			var diceContext = ((DiceParser.DiceExpressionContext)context.d);

			var numOfDice = int.Parse(diceContext.numOfDice.Text);
			var sides = int.Parse(diceContext.sides.Text);
			var threshold = int.Parse(context.NUMBER().GetText());

			bool Condition(int result) => result == threshold;

			return Dice.RollExploding(numOfDice, sides, Condition);
		}
	}
}
