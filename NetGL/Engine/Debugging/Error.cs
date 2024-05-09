using System.Runtime.CompilerServices;

namespace NetGL;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

public static class Error {
    [DoesNotReturn, DebuggerNonUserCode, DebuggerStepThrough, StackTraceHidden, DebuggerHidden]
    public static dynamic type_conversion_error<TFrom, TTo>(TFrom value) =>
        throw new ArgumentException($"Can not convert {typeof(TFrom).Name}:{value} to {typeof(TTo).Name}!");

    [DoesNotReturn, DebuggerNonUserCode, DebuggerStepThrough, StackTraceHidden, DebuggerHidden]
    public static void type_alignment_mismatch<T>(int expected_size, int actual_size) =>
        throw new ArgumentException($"Alignment for {typeof(T).Name} (actual: {actual_size}) does not match std140 (expected: {expected_size})!");

    [DoesNotReturn, DebuggerNonUserCode, DebuggerStepThrough, StackTraceHidden, DebuggerHidden]
    public static void already_disposed<T>(T obj) =>
        throw new ObjectDisposedException(typeof(T).Name, "Object is already free!");

    [DoesNotReturn, DebuggerNonUserCode, DebuggerStepThrough, StackTraceHidden, DebuggerHidden]
    public static void not_allocated<T>(T obj) =>
        throw new Exception($"{typeof(T).Name} is not allocated!");

    [DoesNotReturn, DebuggerNonUserCode, DebuggerStepThrough, StackTraceHidden, DebuggerHidden]
    public static void exception(string message) =>
        throw new Exception(message);


    [DoesNotReturn, DebuggerNonUserCode, DebuggerStepThrough, StackTraceHidden, DebuggerHidden]
    public static void empty_array<T>(string parameter, in T[] array) =>
        throw new System.Exception($"Array is empty: {array.get_type_name()}:{parameter}!");

    [DoesNotReturn, DebuggerNonUserCode, DebuggerStepThrough, StackTraceHidden, DebuggerHidden]
    public static void empty_array<T>(string parameter) =>
        throw new System.Exception($"Array is empty: {typeof(T).get_type_name()}:{parameter}!");

    [DoesNotReturn, DebuggerNonUserCode, DebuggerStepThrough, StackTraceHidden, DebuggerHidden]
    public static void index_out_of_range<T>(T index, T max_index, [CallerMemberName] string caller = default!) =>
        throw new IndexOutOfRangeException($"{caller}({index}) Index out of range ({max_index})!");

    [DebuggerNonUserCode, DebuggerStepThrough, StackTraceHidden, DebuggerHidden]
    public static void index_out_of_range<T>(T index, [CallerMemberName] string caller = default!) =>
        throw new IndexOutOfRangeException($"{caller}({index}) Index out of range!");

    [DebuggerNonUserCode, DebuggerStepThrough, StackTraceHidden, DebuggerHidden]
    public static void index_out_of_range<T>(string param_name, T index, [CallerMemberName] string caller = default!) =>
        throw new IndexOutOfRangeException($"{caller}({index}) {param_name} out of range!");

    [DoesNotReturn, DebuggerNonUserCode, DebuggerStepThrough, StackTraceHidden, DebuggerHidden]
    public static void duplicated_key<T>(T key, [CallerMemberName] string caller = default!) =>
        throw new IndexOutOfRangeException($"{caller}({key}) key already exists!");

    [DoesNotReturn, DebuggerNonUserCode, DebuggerStepThrough, StackTraceHidden, DebuggerHidden]
    public static void invalid_argument<T>(T arg, string? msg = null, [CallerMemberName] string caller = default!)
        => throw new ArgumentException($"{caller} {msg ?? "Invalid argument: "} {typeof(T).get_type_name()} {arg}");
}