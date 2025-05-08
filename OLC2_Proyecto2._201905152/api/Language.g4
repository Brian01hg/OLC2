grammar Language;

program: dcl*;

dcl: varDcl 
    | stmt
    | funcDcl
    | classDcl
    | funcMain
;

varDcl: 'var' ID Tipo '=' expr ';'?
    | 'var'? ID Tipo ';'?
    | 'var' ID '[]' Tipo';'?
    |   ID ':=' expr ';'?
;

classDcl: 'type' ID 'struct' '{' classBody* '}';
classBody: varDcl | funcDcl;

funcDcl: 'func' ID '(' params? ')' Tipo? '{' dcl* '}'
;

funcMain: 'func' 'main' '(' ')' '{' dcl* '}';  

params: param (',' param)*;
param: ID Tipo;

stmt: expr ';'?                                  # ExprStmt
    | 'fmt.Println(' exprList? ')' ';'?         # PrintStmt
    | '{' dcl* '}'                              # BlockStmt
    | 'if'  ( '('? expr ')'? ) stmt ('else' stmt)?                         # IfStmt
    | 'while' '(' expr ')' stmt                                         # WhileStmt
    | 'for' ( '('? forInit expr ';' expr ')'? ) stmt               # ForStmt
    | 'for' expr  stmt                                               # ForStmtCond
    | 'for' ID ',' ID ':=' 'range' ID stmt                # ForRangeStmt
    | 'switch' ( '('? expr ')'? ) '{' caseStmt* defaultStmt? '}'       # SwitchStmt
    | 'break' ';'?                               # BreakStmt
    | 'continue' ';'?                            # ContinueStmt
    | 'return' expr? ';'?                        # ReturnStmt
;

forInit: varDcl | expr;

caseStmt: 'case' expr ':' stmt*;
defaultStmt: 'default' ':' stmt*;

exprList: expr (',' expr)*;

expr:
    '-' expr                                        # Negate
    | expr call+                                     # Callee
    | '!' expr                                      # Not
    | expr op = ('*' | '/' | '%') expr              # MulDiv
    | expr op = ('+' | '-') expr                    # AddSub
    | expr op = ('>' | '<' | '>=' | '<=') expr      # Relational
    | expr op = ('==' | '!=') expr                  # Equality
    | expr '&&' expr                         # LogicalAnd
    | expr '||' expr                         # LogicalOr
    | ID '++'                                # PostIncrement
    | ID '--'                                # PostDecrement
    | expr '=' expr                          # Assign
    | BOOL                      # Boolean
    | FLOAT                     # Float
    | RUNE                      # Rune
    | STRING                    # String
    | INT                       # Int
    | '[]''[]' Tipo '{' arrayList '}'     # ArrayBidimensional
    | '[]' Tipo '{' args? '}'             # Array
    | 'new' ID '(' args? ')'    # New
    | 'nil'                     # NilExpr
    | ID '.' ID  '(' args? ')'  # FuncEmbed
    | ID                        # Identifier
    | '(' expr ')'               # Parens
    | 'slices.Index' '(' ID ',' expr ')' #IndexSlice
    | 'strings.Join' '(' ID ',' expr ')' #Join
    | 'append' '(' ID ',' expr ')' #Append 
    | 'len' '(' ID ')' #Len
;

call : '(' args? ')' #FuncCall
    | '.' ID #Get
    | '[' expr ']' #ArrayAccess
    | '[' expr ']' '[' expr ']' #ArrayAccessBidimensional
;

arrayList: '{' args '}' (',' '{' args '}')*; 
args: expr (',' expr)*
;

Tipo: 'int' | 'float64' | 'string' | 'bool' | 'rune';

INT: [0-9]+;
BOOL: 'true' | 'false';
FLOAT: [0-9]+ '.' [0-9]+;
STRING: '"' (ESC | ~["\\])* '"';
NIL: 'nil';
fragment ESC: '\\' [btnr"\\];
RUNE : '\'' [a-zA-Z0-9] '\'';
ID: [a-zA-Z][a-zA-Z0-9_]*;
WS: [ \t\r\n]+ -> skip;

COMMENT: '//' ~[\r\n]* -> skip;
COMMENT_BLOCK: '/*' .*? '*/' -> skip;