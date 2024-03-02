using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace NetGL.Debug;

public class DebugListener: TextWriter {
    #pragma warning disable CS0169 // Field is never used
    #pragma warning disable CS8602 // Dereference of a possibly null reference.
    #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    #pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.

    public bool log_source = true;

    private readonly TextWriter std_console;

    public DebugListener() {
        std_console = Console.Out;
        Console.SetOut(this);
    }

    public override void Write(string? message) => WriteLine(message);

    public override void WriteLine(string? message) {
        if (message != null) {
            if (log_source) {
                if(DebugConsole.text_filter != "" && !message.Contains(DebugConsole.text_filter, StringComparison.OrdinalIgnoreCase))
                    return;

                if (message.StartsWith('\n')) {
                    std_console.WriteLine();
                    message = message.TrimStart();
                }

                var method = new StackTrace().GetFrame(3).GetMethod();
                var source = $"{method.DeclaringType.Name}.{method.Name}";
                write_line_colored($"[{source}] {message}");
                DebugConsole.log(source, message);
            } else {
                write_line_colored(message);
                DebugConsole.log(message);
            }
        }
    }

    private void write_line_colored(string message) {
        var pattern = @"\[(?:color:)?(\w+)\]";
        var matches = Regex.Matches(message, pattern, RegexOptions.IgnoreCase);

        int lastPos = 0;
        var defaultColor = Console.ForegroundColor;

        foreach (Match match in matches) {
            // Write the text before the color tag
            std_console.Write(message.Substring(lastPos, match.Index - lastPos));

            if (Enum.TryParse<ConsoleColor>(match.Groups[1].Value, true, out var color)) {
                Console.ForegroundColor = color;
            } else {
                Console.ForegroundColor = defaultColor;
            }

            lastPos = match.Index + match.Length;

            // Find the next tag or end of the string
            var nextTag = message.IndexOf("[", lastPos, StringComparison.OrdinalIgnoreCase);
            var textEnd = nextTag >= 0 ? nextTag : message.Length;


            // Write the text in the specified color
            std_console.Write(message.Substring(lastPos, textEnd - lastPos));

            // Update lastPos
            lastPos = textEnd;
        }

        // Reset the color and write any remaining message
        Console.ForegroundColor = defaultColor;
        if (lastPos < message.Length) {
            std_console.WriteLine(message.Substring(lastPos));
        } else {
            std_console.WriteLine();
        }
    }

    public override Encoding Encoding => std_console.Encoding;
}