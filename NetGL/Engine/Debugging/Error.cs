using Microsoft.CodeAnalysis.Scripting;

namespace NetGL;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

public static class Error {
    [DoesNotReturn, DebuggerNonUserCode, DebuggerStepThrough, StackTraceHidden, DebuggerHidden]
    public static dynamic type_conversion_error<TFrom, TTo>(TFrom value) =>
        throw new ArgumentException($"Can not convert {typeof(TFrom).Name}:{value} to {typeof(TTo).Name}!");

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
    public static void index_out_of_range<T>(string parameter, T index) =>
        throw new IndexOutOfRangeException($"Index out of range: {parameter}:{index}!");

    [DoesNotReturn, DebuggerNonUserCode, DebuggerStepThrough, StackTraceHidden, DebuggerHidden]
    public static Exception index_out_of_range<T>(T index, T max_index) =>
        throw new IndexOutOfRangeException($"Index out of range: {index} >= {max_index}!");

    [DoesNotReturn, DebuggerNonUserCode, DebuggerStepThrough, StackTraceHidden, DebuggerHidden]
    public static void empty_array<T>(string parameter, in T[] array) =>
        throw new System.Exception($"Array is empty: {array.get_type_name()}:{parameter}!");

    [DoesNotReturn, DebuggerNonUserCode, DebuggerStepThrough, StackTraceHidden, DebuggerHidden]
    public static void empty_array<T>(string parameter) =>
        throw new System.Exception($"Array is empty: {typeof(T).get_type_name()}:{parameter}!");

    [DebuggerNonUserCode, DebuggerStepThrough, StackTraceHidden, DebuggerHidden]
    public static Exception index_out_of_range<T>(T index) =>
        throw new IndexOutOfRangeException($"Index out of range: {index}!");
}