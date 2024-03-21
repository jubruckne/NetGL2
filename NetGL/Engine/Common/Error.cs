using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using OpenTK.Graphics.OpenGL4;

namespace NetGL;

public static class Error {
    [DoesNotReturn, DebuggerNonUserCode, DebuggerStepThrough, StackTraceHidden, DebuggerHidden]
    public static dynamic type_conversion_error<TFrom, TTo>(TFrom value) =>
        throw new ArgumentException($"Can not convert {typeof(TFrom).Name}:{value} to {typeof(TTo).Name}!");

    [DoesNotReturn, DebuggerNonUserCode, DebuggerStepThrough, StackTraceHidden, DebuggerHidden]
    public static void already_disposed<T>(T obj) =>
        throw new ObjectDisposedException(typeof(T).Name, "Object is already free!");

    [DoesNotReturn, DebuggerNonUserCode, DebuggerStepThrough, StackTraceHidden, DebuggerHidden]
    public static void exception(string message) =>
        throw new Exception(message);

    [DoesNotReturn, DebuggerNonUserCode, DebuggerStepThrough, StackTraceHidden, DebuggerHidden]
    public static void index_out_of_range<T>(string parameter, T index) =>
        throw new IndexOutOfRangeException($"Index out of range: {parameter}:{index}!");

    [DoesNotReturn, DebuggerNonUserCode, DebuggerStepThrough, StackTraceHidden, DebuggerHidden]
    public static void index_out_of_range<T>(T index, T max_index) =>
        throw new IndexOutOfRangeException($"Index out of range: {index} >= {max_index}!");

    [DoesNotReturn, DebuggerNonUserCode, DebuggerStepThrough, StackTraceHidden, DebuggerHidden]
    public static void empty_array<T>(string parameter, in T[] array) =>
        throw new System.Exception($"Array is empty: {array.get_type_name()}:{parameter}!");

    [DoesNotReturn, DebuggerNonUserCode, DebuggerStepThrough, StackTraceHidden, DebuggerHidden]
    public static void empty_array<T>(string parameter) =>
        throw new System.Exception($"Array is empty: {typeof(T).get_type_name()}:{parameter}!");

    [DoesNotReturn, DebuggerNonUserCode, DebuggerStepThrough, StackTraceHidden, DebuggerHidden]
    public static void index_out_of_range<T>(T index) {
        throw new IndexOutOfRangeException($"Index out of range: {index}!");
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
    public static bool assert_opengl<T>(T source) {
        var err = GL.GetError();
        var msg = "";

        while (err != ErrorCode.NoError) {
            Console.WriteLine("Error: " + err);
            msg += err + " ";

            err = GL.GetError();
        }

        if (msg == "") return true;

        throw new Exception($"OpenGL: {msg}! ({source})");
    }

    [DebuggerNonUserCode, DebuggerStepThrough, StackTraceHidden, DebuggerHidden]
    public static bool assert_opengl() {
        var err = GL.GetError();
        var msg = "";

        while (err != ErrorCode.NoError) {
            Console.WriteLine("Error: " + err);
            msg += err + " ";

            err = GL.GetError();
        }

        if (msg == "")
            return true;

        throw new Exception($"OpenGL: {msg}!");
    }
}