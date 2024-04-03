namespace NetGL;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using OpenTK.Graphics.OpenGL4;
using Spectre.Console;

public static class Debug {
    public static void println() {
        AnsiConsole.WriteLine();
    }

    public static void println(string message) {
        AnsiConsole.WriteLine(message);
    }

    public static void println(string message, ConsoleColor color) {
        AnsiConsole.Foreground = color;
        AnsiConsole.WriteLine(message);
        AnsiConsole.ResetColors();
    }

    public static void println(ISpanFormattable message) {
        AnsiConsole.WriteLine();
    }

    public static void println<T>(T obj) {
        AnsiConsole.WriteLine($"{typeof(T).Name} = {obj?.ToString() ?? "<null>"}");
    }

    public static void println<T>(T obj, ConsoleColor color) where T: struct {
        AnsiConsole.Foreground = color;
        AnsiConsole.WriteLine($"{typeof(T).Name} = {obj.ToString() ?? "<null>"}");
        AnsiConsole.ResetColors();
    }

    public static void println<T>(IEnumerable<T> enumerable, ConsoleColor color) {
        AnsiConsole.Foreground = color;
        var items = enumerable.ToArray();
        AnsiConsole.WriteLine($"{typeof(T).get_type_name()}[{items.Length}]: ");

        int i = -1;

        if (items.Length <= 5) {
            foreach (var item in items)
                AnsiConsole.WriteLine($"  {++i}: {item?.ToString() ?? "<null>"}");
        } else {
            foreach (var item in items[0..3])
                AnsiConsole.WriteLine($"  {++i}: {item?.ToString() ?? "<null>"}");
            AnsiConsole.WriteLine("  [...]");
            i = items.Length - 4;
            foreach (var item in items[^3..])
                AnsiConsole.WriteLine($"  {++i}: {item?.ToString() ?? "<null>"}");
        }

        AnsiConsole.ResetColors();
        AnsiConsole.WriteLine();
    }


    [DebuggerNonUserCode, DebuggerStepThrough, StackTraceHidden, DebuggerHidden]
    public static bool assert([DoesNotReturnIf(false)]bool condition, [CallerArgumentExpression("condition")] string? name = default) {
        if (!condition)
            throw new ApplicationException($"Assertion failed: {name}!");
        return true;
    }

    [DebuggerNonUserCode, DebuggerStepThrough, StackTraceHidden, DebuggerHidden]
    public static void assert<T>(T obj, [DoesNotReturnIf(false)]bool condition, [CallerArgumentExpression("condition")] string? name = default) where T: notnull {
        if (!condition)
            throw new Exception($"Assertion failed: {name}! ({obj.ToString()})");
    }

    [DebuggerNonUserCode, DebuggerStepThrough, StackTraceHidden, DebuggerHidden]
    public static void assert_equal<T>(
        T value1,
        T value2,
        [CallerArgumentExpression("value1")] string? name1 = default,
        [CallerArgumentExpression("value2")] string? name2 = default) where T: unmanaged, IEqualityOperators<T, T, bool> {
        if (value1 != value2)
            throw new Exception($"Assertion failed: {name1}=={name2}: {value1}!={value2}!");
    }

    [DebuggerNonUserCode, DebuggerStepThrough, StackTraceHidden, DebuggerHidden]
    public static void assert_not_null<T>(
        T? value,
        [CallerArgumentExpression("value")] string? name = default) where T: class {
        if (value == null)
            throw new Exception($"Assertion failed: {typeof(T).Name} {name}==null!");
    }

    [DebuggerNonUserCode, DebuggerStepThrough, StackTraceHidden, DebuggerHidden]
    public static void assert_not_equal<T>(
        T value1,
        T value2,
        [CallerArgumentExpression("value1")] string? name1 = default,
        [CallerArgumentExpression("value2")] string? name2 = default) where T: unmanaged, IEqualityOperators<T, T, bool> {
        if (value1 == value2)
            throw new Exception($"Assertion failed: {name1}!={name2}: {value1}=={value2}!");
    }


    [DebuggerNonUserCode, DebuggerStepThrough, StackTraceHidden, DebuggerHidden]
    public static void assert_opengl<T>(T source) {
        var err = GL.GetError();
        var msg = "";

        while (err != ErrorCode.NoError) {
            Console.WriteLine("Error: " + err);
            msg += err + " ";

            err = GL.GetError();
        }

        if (msg == "") return;

        throw new Exception($"OpenGL: {msg}! ({source})");
    }

    [DebuggerNonUserCode, DebuggerStepThrough, StackTraceHidden, DebuggerHidden]
    public static void assert_opengl() {
        var err = GL.GetError();
        var msg = "";

        while (err != ErrorCode.NoError) {
            Console.WriteLine("Error: " + err);
            msg += err + " ";

            err = GL.GetError();
        }

        if (msg == "")
            return;

        throw new Exception($"OpenGL: {msg}!");
    }
}