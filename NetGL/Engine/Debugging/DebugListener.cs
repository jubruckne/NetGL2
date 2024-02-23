using System.Diagnostics;
using System.Text;

namespace NetGL.Debug;

public class DebugListener: TextWriter {
    #pragma warning disable CS0169 // Field is never used
    #pragma warning disable CS8602 // Dereference of a possibly null reference.
    #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    #pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.

    public bool log_source = true;

    private readonly TextWriter console;

    public DebugListener() {
        console = Console.Out;
        Console.SetOut(this);
    }

    public override void Write(string? message) => WriteLine(message);

    public override void WriteLine(string? message) {
        if (message != null) {
            if (log_source) {
                if(DebugConsole.text_filter != "" && !message.Contains(DebugConsole.text_filter, StringComparison.OrdinalIgnoreCase))
                    return;

                if (message.StartsWith('\n')) {
                    console.WriteLine();
                    message = message.TrimStart();
                }

                var method = new StackTrace().GetFrame(3).GetMethod();
                var source = $"{method.DeclaringType.Name}.{method.Name}";
                console.WriteLine($"[{source}]: {message}");
                DebugConsole.log(source, message);
            } else {
                console.WriteLine(message);
                DebugConsole.log(message);
            }
        }
    }

    public override Encoding Encoding => console.Encoding;
}