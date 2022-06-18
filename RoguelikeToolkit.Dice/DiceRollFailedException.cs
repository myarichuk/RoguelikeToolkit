using System;
using System.Runtime.Serialization;

namespace RoguelikeToolkit.DiceExpression
{
	public class DiceRollFailedException : Exception
	{
		public DiceRollFailedException(Exception innerException) : base("Failed to roll dice expression", innerException)
		{
		}

		public DiceRollFailedException(string message) : base(message)
		{
		}

		public DiceRollFailedException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected DiceRollFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
