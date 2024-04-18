using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace NetGL;

public class Script {
    private readonly string code;
    private readonly Script<global::Engine> executable;

    public Script(string code) {
        this.code = code;
        this.executable = compile();
    }

    private Script<global::Engine> compile() {
        try {
            var scriptOptions = ScriptOptions.Default
                .AddImports("System")
                .AddReferences(typeof(Console).Assembly)
                .AddReferences(typeof(global::Engine).Assembly);

            return CSharpScript.Create<global::Engine>(code, scriptOptions, typeof(global::Engine));
        }
        catch (CompilationErrorException e) {
            Console.WriteLine($"Script execution error: {string.Join("\n", e.Diagnostics)}");
            throw;
        }
    }

    public void run(global::Engine engine) {
        executable.RunAsync(engine).Wait();
    }
}