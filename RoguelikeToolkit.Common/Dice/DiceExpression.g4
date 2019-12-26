grammar DiceExpression;

PLUS: '+';
MINUS: '-';
MULTIPLY: '*';
DIVIDE: '/';
NUMBER: [0-9]+;
DICE: NUMBER ('d'|'D');
KEEP: NUMBER ('k' | 'K');
LPAREN: '(';
RPAREN: ')';
LOW: ('l' | 'L');
HIGH: ('h' | 'H');
 

diceExpression: expression EOF;
 
expression:
	NUMBER																							   #NumberExpression |	
	(additiveModifier = NUMBER)? die = DIE keep = KEEP? (op = (PLUS | MINUS) pips = NUMBER)?		   #DieExpression |
	(additiveModifier = NUMBER)? die = DIE keep = KEEP? (op = (PLUS | MINUS) lowHigh = (LOW | HIGH))?  #SelectiveDieExpression |
	LPAREN expression RPAREN																		   #ParenthesisExpression |
	left = expression op = (MULTIPLY | DIVIDE) right = expression									   #MultiplyDivideExpression |
	left = expression op = (PLUS | MINUS) right = expression										   #PlusMinusExpression
	;