using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
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
    public static void exception(string message) =>
        throw new System.Exception(message);

    [DoesNotReturn, StackTraceHidden]
    public static void index_out_of_range<T>(string parameter, T index) =>
        throw new IndexOutOfRangeException($"Index out of range: {parameter}:{index}!");

    [DoesNotReturn, StackTraceHidden]
    public static void index_out_of_range<T>(T index, T max_index) =>
        throw new IndexOutOfRangeException($"Index out of range: {index} >= {max_index}!");

    [DoesNotReturn, StackTraceHidden]
    public static void empty_array<T>(string parameter, in T[] array) =>
        throw new System.Exception($"Array is empty: {array.get_type_name()}:{parameter}!");

    [DoesNotReturn, StackTraceHidden]
    public static void empty_array<T>(string parameter) =>
        throw new System.Exception($"Array is empty: {typeof(T).get_type_name()}:{parameter}!");

    [DoesNotReturn, StackTraceHidden]
    public static void index_out_of_range<T>(T index) {
        throw new IndexOutOfRangeException($"Index out of range: {index}!");
    }

    public class WrongContextException : Exception {
        public WrongContextException(string expected, string current) : base(0, $"{expected} is not current! (current: {current})") {
        }
    }

    public static void assert(bool condition, [CallerArgumentExpression("condition")] string? name = default) {
        if (!condition)
            throw new Exception($"Assertion failed: {name}!");
    }

    public static void assert<T>(T obj, bool condition, [CallerArgumentExpression("condition")] string? name = default) where T: notnull {
        if (!condition)
            throw new Exception($"Assertion failed: {name}! {obj.ToString()}");
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