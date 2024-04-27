grammar shader_lang;

// Lexer Rules
SHADER         : 'shader';
VERTEX         : 'vertex';
UNIFORM        : 'uniform';
VOID           : 'void';
USING          : 'using';
OUT            : 'out';
IN             : 'in';
LCURLY         : '{';
RCURLY         : '}';
LPAREN         : '(';
RPAREN         : ')';
SEMI           : ';';
COMMA          : ',';
DOT            : '.';
EQUALS         : '=';
VEC2           : 'vec2';
VEC3           : 'vec3';
VEC4           : 'vec4';
MAT4           : 'mat4';
SAMPLER2D      : 'sampler2D';
VERTEX_STAGE   : 'vertex_stage';
IDENTIFIER     : [a-zA-Z_][a-zA-Z0-9_]*;
STRING         : '"' ( ~["\\] | '\\' . )* '"';
WS             : [ \t\r\n]+ -> skip;
LINE_COMMENT   : '//' ~[\r\n]* -> skip;
BLOCK_COMMENT  : '/*' .*? '*/' -> skip;

// Parser Rules
shader         : shaderDecl usingDecls vertexBlock uniformBlock vertexStage;
shaderDecl     : SHADER STRING SEMI;
usingDecls     : (usingDecl)*;
usingDecl      : USING STRING SEMI;

vertexBlock    : VERTEX LCURLY vertexDecls RCURLY;
vertexDecls    : (vertexDecl)*;
vertexDecl     : type IDENTIFIER SEMI;
type           : VEC2 | VEC3 | VEC4 | MAT4 | SAMPLER2D;

uniformBlock   : UNIFORM LCURLY uniformDecls RCURLY;
uniformDecls   : (uniformDecl)*;
uniformDecl    : type IDENTIFIER SEMI;

vertexStage    : VOID VERTEX_STAGE LPAREN vertexStageParams RPAREN LCURLY vertexBody RCURLY;
vertexStageParams   : OUT vertexStageParam (COMMA OUT vertexStageParam)*;
vertexStageParam    : type IDENTIFIER;

vertexBody     : statement+;

statement      : assignment SEMI;
assignment     : IDENTIFIER EQUALS IDENTIFIER;