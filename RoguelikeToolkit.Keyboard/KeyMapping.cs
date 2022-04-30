#nullable enable
using System;
using System.Collections.Generic;

namespace RoguelikeToolkit.Keyboard
{
	// support up to three key combinations
	// (it is unlikely more than three key combination would be needed)
	internal class KeyMapping<KeyEnum, ActionData> where KeyEnum : Enum
	{
		public KeyEnum? FirstKey { get; set; }
	
		public KeyEnum? SecondKey { get; set; }

		public KeyEnum? ThirdKey { get; set; }
		
		public ActionData? Action { get; set; }

		public override bool Equals(object? obj)
		{
			return obj is KeyMapping<KeyEnum, ActionData> combination &&
				   EqualityComparer<KeyEnum>.Default.Equals(FirstKey, combination.FirstKey) &&
				   EqualityComparer<KeyEnum>.Default.Equals(SecondKey, combination.SecondKey) &&
				   EqualityComparer<KeyEnum>.Default.Equals(ThirdKey, combination.ThirdKey);
		}

		public override int GetHashCode() =>
			HashCode.Combine(FirstKey, SecondKey, ThirdKey);
	}
}
