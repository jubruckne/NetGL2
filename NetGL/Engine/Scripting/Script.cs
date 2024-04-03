using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace NetGL;

public class Script: IAssetType<Script> {
    public static string path => "Scripts";

    private readonly string code;
    private readonly Script<Engine> executable;

    public Script(string code) {
        this.code = code;
        this.executable = compile();
    }

    private Script<Engine> compile() {
        try {
            var scriptOptions = ScriptOptions.Default
                .AddImports("System")
                .AddReferences(typeof(Console).Assembly)
                .AddReferences(typeof(Engine).Assembly);

            return CSharpScript.Create<Engine>(code, scriptOptions, typeof(Engine));
        }
        catch (CompilationErrorException e) {
            Console.WriteLine($"Script execution error: {string.Join("\n", e.Diagnostics)}");
            throw;
        }
    }

    public void run(Engine engine) {
        executable.RunAsync(engine).Wait();
    }

    static Script IAssetType<Script>.load_from_file(string filename) {
        return new Script(File.ReadAllText(filename));
    }
}