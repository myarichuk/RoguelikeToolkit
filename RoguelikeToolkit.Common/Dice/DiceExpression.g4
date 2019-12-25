grammar DiceExpression;

PLUS: '+';
MINUS: '-';
MULTIPLY: '*';
DIVIDE: '/';
NUMBER: [0-9]+;
DICE: NUMBER ('d'|'D');
LPAREN: '(';
RPAREN: ')';


diceExpression: expression EOF;

expression:
	NUMBER																		#NumberExpression |	
	(additiveModifier = NUMBER)? die = DIE (op = (PLUS | MINUS) pips = NUMBER)?	#DieExpression |
	LPAREN expression RPAREN													#ParenthesisExpression |
	left = expression op = (MULTIPLY | DIVIDE) right = expression				#MultiplyDivideExpression |
	left = expression op = (PLUS | MINUS) right = expression					#PlusMinusExpression
	;