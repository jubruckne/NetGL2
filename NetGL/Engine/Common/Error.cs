using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using OpenTK.Graphics.OpenGL4;

namespace NetGL;

public static class Error {
    public class Exception: System.Exception {
        public readonly int code;

        public Exception(string message): base(message) { }

        public Exception(int code, string message): base(message) {
            this.code = code;
        }
    }

    [DoesNotReturn, StackTraceHidden]
    public static void type_conversion_error<TFrom, TTo>(TFrom value) =>
        throw new ArgumentException($"Can not convert {typeof(TFrom).Name}:{value} to {typeof(TTo).Name}!");

    [DoesNotReturn, StackTraceHidden]
    public static void already_disposed<T>(T obj) =>
        throw new ObjectDisposedException(typeof(T).Name, "Object is already free!");

    [DoesNotReturn, StackTraceHidden]
    public static void index_out_of_range<T>(string parameter, T index) =>
        throw new IndexOutOfRangeException($"Index out of range: {parameter}:{index}!");

    [DoesNotReturn, StackTraceHidden]
    public static void index_out_of_range<T>(T index) =>
        throw new IndexOutOfRangeException($"Index out of range: {index}!");

    public class WrongContextException : Exception {
        public WrongContextException(string expected, string current) : base(0, $"{expected} is not current! (current: {current})") {
        }
    }

    public static bool check(bool throw_exception = true) {
        var err = GL.GetError();
        var msg = "";

        while (err != ErrorCode.NoError) {
            Console.WriteLine("Error: " + err);
            msg += err + " ";

            err = GL.GetError();
        }

        if (msg == "")
            return true;

        if (throw_exception)
            throw new Exception(123, msg);

        return false;
    }
}