using Microsoft.CodeAnalysis.CSharp.Syntax;
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

    public static void already_disposed<T>(T obj) =>
        throw new ObjectDisposedException(typeof(T).Name, "Object is already free!");

    public static void index_out_of_range<T>(string parameter, T index) =>
        throw new IndexOutOfRangeException($"Index out of range: {parameter}:{index}!");

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