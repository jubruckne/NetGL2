/*using Antlr4.Runtime;
using OpenTK.Windowing.Common;

var input   = new AntlrFileStream("/Users/julia/RiderProjects/NetGL/NetGL/Assets/test.shader");
var lexer   = new shader_langLexer(input);
var tokens  = new CommonTokenStream(lexer);
var parser  = new shader_langParser(tokens);
var context = parser.shader();  // Start parsing at the root rule
// You can now use the context to navigate the parsed structure or perform further analysis
Console.WriteLine(context.ToStringTree(parser));

foreach (var token in tokens.GetTokens())
    Console.WriteLine($"{token.ToString()}: {token.Type}");
*/

using OpenTK.Windowing.Common;

Engine engine = new("NetGL", (1280, 820), debugging: true, window_state: WindowState.Normal);

//if (Selftest.run())
engine.Run();