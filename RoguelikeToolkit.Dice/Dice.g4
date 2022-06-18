/* syntax inspired by https://github.com/brianbruggeman/dice, notation taken from https://en.wikipedia.org/wiki/Dice_notation */

grammar Dice;

NUMBER: [0-9]+;
DICE: ('d'|'D');
KEEP: ('k'|'K');

LPAREN: '(';
RPAREN: ')';

PERCENT: '%';
PLUS: '+';
MINUS: '-';
MULTIPLY: '*';
DIVIDE: '/';
EXCLAMATION: '!';
GREATER: '>';
LESSER: '<';

WS
   : [ \t\n\r] + -> skip
   ;

root: dice EOF;
dice: 
	
		LPAREN dice RPAREN													 #DiceParenthesisExpression
	|	numOfDice = NUMBER DICE sides = NUMBER (KEEP keepNum = NUMBER)?		 #DiceExpression
	|   numOfDice = NUMBER KEEP keepNum = NUMBER							 #Dice10KeepExpression /* roll 'numOfDice' of 10-dice cubes, keep 'keepNum' highest rolls and sum them  */
	|	d = dice EXCLAMATION												 #ExplodingConstantDiceExpression
	|	d = dice EXCLAMATION NUMBER											 #ExplodingThresholdDiceExpression
	|	d = dice EXCLAMATION op = (GREATER | LESSER) NUMBER					 #ExplodingConditionalDiceExpression
	|	DICE PERCENT														 #Dice100Expression /* roll 'percentage' dice */
	|	DICE sides = NUMBER (KEEP keepNum = NUMBER)?						 #OneDiceExpression
	|	left = dice op = (MULTIPLY | DIVIDE) right = dice					 #DiceMultiplyDivideExpression
	|	left = dice op = (PLUS | MINUS) right = dice						 #DiceAddSubstractExpression
	|   NUMBER																 #DiceConstantException
	;
