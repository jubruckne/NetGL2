//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.13.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from /Users/julia/RiderProjects/NetGL/NetGL/Assets/shader_lang.g4 by ANTLR 4.13.1

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

using System;
using System.IO;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using DFA = Antlr4.Runtime.Dfa.DFA;

[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.13.1")]
[System.CLSCompliant(false)]
public partial class shader_langLexer : Lexer {
	protected static DFA[] decisionToDFA;
	protected static PredictionContextCache sharedContextCache = new PredictionContextCache();
	public const int
		SHADER=1, VERTEX=2, UNIFORM=3, VOID=4, USING=5, OUT=6, IN=7, LCURLY=8, 
		RCURLY=9, LPAREN=10, RPAREN=11, SEMI=12, COMMA=13, DOT=14, EQUALS=15, 
		VEC2=16, VEC3=17, VEC4=18, MAT4=19, SAMPLER2D=20, VERTEX_STAGE=21, IDENTIFIER=22, 
		STRING=23, WS=24, LINE_COMMENT=25, BLOCK_COMMENT=26;
	public static string[] channelNames = {
		"DEFAULT_TOKEN_CHANNEL", "HIDDEN"
	};

	public static string[] modeNames = {
		"DEFAULT_MODE"
	};

	public static readonly string[] ruleNames = {
		"SHADER", "VERTEX", "UNIFORM", "VOID", "USING", "OUT", "IN", "LCURLY", 
		"RCURLY", "LPAREN", "RPAREN", "SEMI", "COMMA", "DOT", "EQUALS", "VEC2", 
		"VEC3", "VEC4", "MAT4", "SAMPLER2D", "VERTEX_STAGE", "IDENTIFIER", "STRING", 
		"WS", "LINE_COMMENT", "BLOCK_COMMENT"
	};


	public shader_langLexer(ICharStream input)
	: this(input, Console.Out, Console.Error) { }

	public shader_langLexer(ICharStream input, TextWriter output, TextWriter errorOutput)
	: base(input, output, errorOutput)
	{
		Interpreter = new LexerATNSimulator(this, _ATN, decisionToDFA, sharedContextCache);
	}

	private static readonly string[] _LiteralNames = {
		null, "'shader'", "'vertex'", "'uniform'", "'void'", "'using'", "'out'", 
		"'in'", "'{'", "'}'", "'('", "')'", "';'", "','", "'.'", "'='", "'vec2'", 
		"'vec3'", "'vec4'", "'mat4'", "'sampler2D'", "'vertex_stage'"
	};
	private static readonly string[] _SymbolicNames = {
		null, "SHADER", "VERTEX", "UNIFORM", "VOID", "USING", "OUT", "IN", "LCURLY", 
		"RCURLY", "LPAREN", "RPAREN", "SEMI", "COMMA", "DOT", "EQUALS", "VEC2", 
		"VEC3", "VEC4", "MAT4", "SAMPLER2D", "VERTEX_STAGE", "IDENTIFIER", "STRING", 
		"WS", "LINE_COMMENT", "BLOCK_COMMENT"
	};
	public static readonly IVocabulary DefaultVocabulary = new Vocabulary(_LiteralNames, _SymbolicNames);

	[NotNull]
	public override IVocabulary Vocabulary
	{
		get
		{
			return DefaultVocabulary;
		}
	}

	public override string GrammarFileName { get { return "shader_lang.g4"; } }

	public override string[] RuleNames { get { return ruleNames; } }

	public override string[] ChannelNames { get { return channelNames; } }

	public override string[] ModeNames { get { return modeNames; } }

	public override int[] SerializedAtn { get { return _serializedATN; } }

	static shader_langLexer() {
		decisionToDFA = new DFA[_ATN.NumberOfDecisions];
		for (int i = 0; i < _ATN.NumberOfDecisions; i++) {
			decisionToDFA[i] = new DFA(_ATN.GetDecisionState(i), i);
		}
	}
	private static int[] _serializedATN = {
		4,0,26,202,6,-1,2,0,7,0,2,1,7,1,2,2,7,2,2,3,7,3,2,4,7,4,2,5,7,5,2,6,7,
		6,2,7,7,7,2,8,7,8,2,9,7,9,2,10,7,10,2,11,7,11,2,12,7,12,2,13,7,13,2,14,
		7,14,2,15,7,15,2,16,7,16,2,17,7,17,2,18,7,18,2,19,7,19,2,20,7,20,2,21,
		7,21,2,22,7,22,2,23,7,23,2,24,7,24,2,25,7,25,1,0,1,0,1,0,1,0,1,0,1,0,1,
		0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,2,1,2,1,2,1,2,1,2,1,2,1,2,1,2,1,3,1,3,
		1,3,1,3,1,3,1,4,1,4,1,4,1,4,1,4,1,4,1,5,1,5,1,5,1,5,1,6,1,6,1,6,1,7,1,
		7,1,8,1,8,1,9,1,9,1,10,1,10,1,11,1,11,1,12,1,12,1,13,1,13,1,14,1,14,1,
		15,1,15,1,15,1,15,1,15,1,16,1,16,1,16,1,16,1,16,1,17,1,17,1,17,1,17,1,
		17,1,18,1,18,1,18,1,18,1,18,1,19,1,19,1,19,1,19,1,19,1,19,1,19,1,19,1,
		19,1,19,1,20,1,20,1,20,1,20,1,20,1,20,1,20,1,20,1,20,1,20,1,20,1,20,1,
		20,1,21,1,21,5,21,155,8,21,10,21,12,21,158,9,21,1,22,1,22,1,22,1,22,5,
		22,164,8,22,10,22,12,22,167,9,22,1,22,1,22,1,23,4,23,172,8,23,11,23,12,
		23,173,1,23,1,23,1,24,1,24,1,24,1,24,5,24,182,8,24,10,24,12,24,185,9,24,
		1,24,1,24,1,25,1,25,1,25,1,25,5,25,193,8,25,10,25,12,25,196,9,25,1,25,
		1,25,1,25,1,25,1,25,1,194,0,26,1,1,3,2,5,3,7,4,9,5,11,6,13,7,15,8,17,9,
		19,10,21,11,23,12,25,13,27,14,29,15,31,16,33,17,35,18,37,19,39,20,41,21,
		43,22,45,23,47,24,49,25,51,26,1,0,5,3,0,65,90,95,95,97,122,4,0,48,57,65,
		90,95,95,97,122,2,0,34,34,92,92,3,0,9,10,13,13,32,32,2,0,10,10,13,13,207,
		0,1,1,0,0,0,0,3,1,0,0,0,0,5,1,0,0,0,0,7,1,0,0,0,0,9,1,0,0,0,0,11,1,0,0,
		0,0,13,1,0,0,0,0,15,1,0,0,0,0,17,1,0,0,0,0,19,1,0,0,0,0,21,1,0,0,0,0,23,
		1,0,0,0,0,25,1,0,0,0,0,27,1,0,0,0,0,29,1,0,0,0,0,31,1,0,0,0,0,33,1,0,0,
		0,0,35,1,0,0,0,0,37,1,0,0,0,0,39,1,0,0,0,0,41,1,0,0,0,0,43,1,0,0,0,0,45,
		1,0,0,0,0,47,1,0,0,0,0,49,1,0,0,0,0,51,1,0,0,0,1,53,1,0,0,0,3,60,1,0,0,
		0,5,67,1,0,0,0,7,75,1,0,0,0,9,80,1,0,0,0,11,86,1,0,0,0,13,90,1,0,0,0,15,
		93,1,0,0,0,17,95,1,0,0,0,19,97,1,0,0,0,21,99,1,0,0,0,23,101,1,0,0,0,25,
		103,1,0,0,0,27,105,1,0,0,0,29,107,1,0,0,0,31,109,1,0,0,0,33,114,1,0,0,
		0,35,119,1,0,0,0,37,124,1,0,0,0,39,129,1,0,0,0,41,139,1,0,0,0,43,152,1,
		0,0,0,45,159,1,0,0,0,47,171,1,0,0,0,49,177,1,0,0,0,51,188,1,0,0,0,53,54,
		5,115,0,0,54,55,5,104,0,0,55,56,5,97,0,0,56,57,5,100,0,0,57,58,5,101,0,
		0,58,59,5,114,0,0,59,2,1,0,0,0,60,61,5,118,0,0,61,62,5,101,0,0,62,63,5,
		114,0,0,63,64,5,116,0,0,64,65,5,101,0,0,65,66,5,120,0,0,66,4,1,0,0,0,67,
		68,5,117,0,0,68,69,5,110,0,0,69,70,5,105,0,0,70,71,5,102,0,0,71,72,5,111,
		0,0,72,73,5,114,0,0,73,74,5,109,0,0,74,6,1,0,0,0,75,76,5,118,0,0,76,77,
		5,111,0,0,77,78,5,105,0,0,78,79,5,100,0,0,79,8,1,0,0,0,80,81,5,117,0,0,
		81,82,5,115,0,0,82,83,5,105,0,0,83,84,5,110,0,0,84,85,5,103,0,0,85,10,
		1,0,0,0,86,87,5,111,0,0,87,88,5,117,0,0,88,89,5,116,0,0,89,12,1,0,0,0,
		90,91,5,105,0,0,91,92,5,110,0,0,92,14,1,0,0,0,93,94,5,123,0,0,94,16,1,
		0,0,0,95,96,5,125,0,0,96,18,1,0,0,0,97,98,5,40,0,0,98,20,1,0,0,0,99,100,
		5,41,0,0,100,22,1,0,0,0,101,102,5,59,0,0,102,24,1,0,0,0,103,104,5,44,0,
		0,104,26,1,0,0,0,105,106,5,46,0,0,106,28,1,0,0,0,107,108,5,61,0,0,108,
		30,1,0,0,0,109,110,5,118,0,0,110,111,5,101,0,0,111,112,5,99,0,0,112,113,
		5,50,0,0,113,32,1,0,0,0,114,115,5,118,0,0,115,116,5,101,0,0,116,117,5,
		99,0,0,117,118,5,51,0,0,118,34,1,0,0,0,119,120,5,118,0,0,120,121,5,101,
		0,0,121,122,5,99,0,0,122,123,5,52,0,0,123,36,1,0,0,0,124,125,5,109,0,0,
		125,126,5,97,0,0,126,127,5,116,0,0,127,128,5,52,0,0,128,38,1,0,0,0,129,
		130,5,115,0,0,130,131,5,97,0,0,131,132,5,109,0,0,132,133,5,112,0,0,133,
		134,5,108,0,0,134,135,5,101,0,0,135,136,5,114,0,0,136,137,5,50,0,0,137,
		138,5,68,0,0,138,40,1,0,0,0,139,140,5,118,0,0,140,141,5,101,0,0,141,142,
		5,114,0,0,142,143,5,116,0,0,143,144,5,101,0,0,144,145,5,120,0,0,145,146,
		5,95,0,0,146,147,5,115,0,0,147,148,5,116,0,0,148,149,5,97,0,0,149,150,
		5,103,0,0,150,151,5,101,0,0,151,42,1,0,0,0,152,156,7,0,0,0,153,155,7,1,
		0,0,154,153,1,0,0,0,155,158,1,0,0,0,156,154,1,0,0,0,156,157,1,0,0,0,157,
		44,1,0,0,0,158,156,1,0,0,0,159,165,5,34,0,0,160,164,8,2,0,0,161,162,5,
		92,0,0,162,164,9,0,0,0,163,160,1,0,0,0,163,161,1,0,0,0,164,167,1,0,0,0,
		165,163,1,0,0,0,165,166,1,0,0,0,166,168,1,0,0,0,167,165,1,0,0,0,168,169,
		5,34,0,0,169,46,1,0,0,0,170,172,7,3,0,0,171,170,1,0,0,0,172,173,1,0,0,
		0,173,171,1,0,0,0,173,174,1,0,0,0,174,175,1,0,0,0,175,176,6,23,0,0,176,
		48,1,0,0,0,177,178,5,47,0,0,178,179,5,47,0,0,179,183,1,0,0,0,180,182,8,
		4,0,0,181,180,1,0,0,0,182,185,1,0,0,0,183,181,1,0,0,0,183,184,1,0,0,0,
		184,186,1,0,0,0,185,183,1,0,0,0,186,187,6,24,0,0,187,50,1,0,0,0,188,189,
		5,47,0,0,189,190,5,42,0,0,190,194,1,0,0,0,191,193,9,0,0,0,192,191,1,0,
		0,0,193,196,1,0,0,0,194,195,1,0,0,0,194,192,1,0,0,0,195,197,1,0,0,0,196,
		194,1,0,0,0,197,198,5,42,0,0,198,199,5,47,0,0,199,200,1,0,0,0,200,201,
		6,25,0,0,201,52,1,0,0,0,7,0,156,163,165,173,183,194,1,6,0,0
	};

	public static readonly ATN _ATN =
		new ATNDeserializer().Deserialize(_serializedATN);


}
