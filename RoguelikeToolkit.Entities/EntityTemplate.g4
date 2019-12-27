/** Taken from "The Definitive ANTLR 4 Reference" by Terence Parr */
// Derived from http://json.org

grammar EntityTemplate;

template: object EOF;

object
   : '{' field (',' field)* '}'
   | '{' '}'
   ;

field
   :
        COMPONENTS ':' '[' object (',' object)* ']'  #ComponentsField
   |    INHERITS ':'  ('[' baseTemplates += STRING (',' baseTemplates += STRING)* ']' | '[' ']') #InheritsField
   |    IDENTIFIER ':' id = STRING #IdentifierField
   |    STRING ':' value #JsonField
   ;

array
   : '[' values += value (',' values += value)* ']'
   | '[' ']'
   ;

value
   : STRING
   | NUMBER
   | object
   | array
   | 'true'
   | 'false'
   | 'null'
   ;


IDENTIFIER: '"' I D (E N T I F I E R)? '"';
INHERITS: '"' I N H E R I T S (F R O M)?'"' | '"' B A S E T E M P L A T E S '"';
COMPONENTS: '"' C O M P O N E N T S '"';

STRING
   : '"' (ESC | SAFECODEPOINT)* '"'
   ;


fragment ESC
   : '\\' (["\\/bfnrt] | UNICODE)
   ;


fragment UNICODE
   : 'u' HEX HEX HEX HEX
   ;


fragment HEX
   : [0-9a-fA-F]
   ;


fragment SAFECODEPOINT
   : ~ ["\\\u0000-\u001F]
   ;


NUMBER
   : '-'? INT ('.' [0-9] +)? EXP?
   ;


fragment INT
   : '0' | [1-9] [0-9]*
   ;

// no leading zeros

fragment EXP
   : [Ee] [+\-]? INT
   ;

WS
   : [ \t\n\r] + -> skip
   ;



fragment A : [aA]; // match either an 'a' or 'A'
fragment B : [bB];
fragment C : [cC];
fragment D : [dD];
fragment E : [eE];
fragment F : [fF];
fragment G : [gG];
fragment H : [hH];
fragment I : [iI];
fragment J : [jJ];
fragment K : [kK];
fragment L : [lL];
fragment M : [mM];
fragment N : [nN];
fragment O : [oO];
fragment P : [pP];
fragment Q : [qQ];
fragment R : [rR];
fragment S : [sS];
fragment T : [tT];
fragment U : [uU];
fragment V : [vV];
fragment W : [wW];
fragment X : [xX];
fragment Y : [yY];
fragment Z : [zZ];