using System.Numerics;
using OpenTK.Mathematics;

namespace NetGL;
/*
 MIT License

Copyright (c) 2021 ikorin24

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

/// <summary>
/// Array class which is allocated in unmanaged memory.<para/>
/// Only for unmanaged types. (e.g. int, float, recursive-unmanaged struct, and so on.)
/// </summary>
/// <typeparam name="T">type of array</typeparam>
[DebuggerTypeProxy(typeof(NativeArrayDebuggerTypeProxy<>))]
[DebuggerDisplay("NativeArray<{typeof(T).Name}>[{length}]")]
public sealed unsafe class NativeArray<T> : IEnumerable<T>, IDisposable where T : unmanaged {
    public int rows { get; private set; }
    public int columns { get; private set; }
    public int length { get; private set; }

    private nint data;

    /// <summary>Get the specific item of specific index.</summary>
    /// <param name="index">index</param>
    /// <returns>The item of specific index</returns>
    public T this[int index] {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            if (index < 0 || index >= length) Error.index_out_of_range(index);
            return ((T*)data)[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set {
            if (index < 0 || index >= length) Error.index_out_of_range(index);
            ((T*)data)[index] = value;
        }
    }

    /// <summary>Get the specific item of specific index.</summary>
    /// <param name="row">row</param>
    /// <param name="col">column</param>
    /// <returns>The item of specific index</returns>
    public T this[int col, int row] {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this[row * columns + col];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => this[row * columns + col] = value;
    }

    public NativeView<V> get_view<V>() where V : unmanaged {
        return new(
            data,
            sizeof(V),
            length
        );
    }

    public NativeView<V> get_view<V>(nint element_offset) where V : unmanaged {
        if (element_offset > sizeof(T)) Error.index_out_of_range(nameof(element_offset), element_offset);
        return new(
            data + element_offset,
            sizeof(T),
            length
        );
    }

    public NativeView<V> get_view<V>(string element_name) where V : unmanaged {
        return new(
            data + Marshal.OffsetOf<T>(element_name),
            sizeof(T),
            length
        );
    }

    /// <summary>Create new <see cref="NativeArray{T}"/></summary>
    /// <param name="columns">number of columns</param>
    /// <param name="rows">number of rows</param>
    public NativeArray(int columns, int rows) : this(rows * columns) {
        this.rows = rows;
        this.columns = columns;
    }

    /// <summary>Create new <see cref="NativeArray{T}"/></summary>
    /// <param name="length">length of array</param>
    public NativeArray(int length) {
        if (length < 0) {
            Error.index_out_of_range(nameof(length), length);
        }

        var bytes = sizeof(T) * length;
        if (bytes < 0) {
            Error.index_out_of_range(nameof(T), bytes);
        }

        this.data = (IntPtr)NativeMemory.AlignedAlloc((UIntPtr)bytes, 16);

        this.length = length;
        this.rows = length;
        this.columns = 1;
    }

    /// <summary>Create new <see cref="NativeArray{T}"/>, those elements are copied from <see cref="ReadOnlySpan{T}"/>.</summary>
    /// <param name="span">Elements of the <see cref="NativeArray{T}"/> are initialized by this <see cref="ReadOnlySpan{T}"/>.</param>
    public NativeArray(ReadOnlySpan<T> span) : this(span.Length) {
        span.CopyTo(new Span<T>((void*)data, length));
    }

    public void resize(int new_length) {
        if (new_length == length) return;

        if (new_length < 0 || new_length < length) {
            Error.index_out_of_range(nameof(length), length);
        }

        var obj_size = sizeof(T);
        var bytes_old = length * obj_size;
        var bytes_new = new_length * obj_size;

        if (bytes_old < 0) {
            Error.index_out_of_range(nameof(T), bytes_old);
        }

        var new_array = (IntPtr)NativeMemory.AlignedAlloc((UIntPtr)bytes_new, 16);

        System.Buffer.MemoryCopy((void*)data, (void*)new_array, bytes_new, bytes_old);

        clear();

        this.data = new_array;
        this.length = new_length;
        this.rows = length;
        this.columns = 1;
    }

    /// <summary>Finalizer of <see cref="NativeArray{T}"/></summary>
    ~NativeArray() => clear();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool is_disposed() => data == IntPtr.Zero;

    /// <summary>Get pointer address of this array.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public nint get_pointer() => data;

    IEnumerator<T> IEnumerable<T>.GetEnumerator() {
        if (is_disposed()) Error.already_disposed(this);
        // Avoid boxing by using class enumerator.
        return new Enumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator() {
        if (is_disposed()) Error.already_disposed(this);
        // Avoid boxing by using class enumerator.
        return new Enumerator(this);
    }

    public void insert(in T[] items, int at = 0) {
        if (at < 0)
            Error.index_out_of_range("at", at);

        items.CopyTo(as_span(at));
    }

    /// <summary>Copy to managed memory</summary>
    /// <param name="destination">managed memory array</param>
    /// <param name="arrayIndex">start index of destination array</param>
    public void copy_to(T[] destination, int arrayIndex) {
        if (is_disposed()) Error.already_disposed(this);
        if ((uint)arrayIndex >= (uint)destination.Length) {
            throw new ArgumentOutOfRangeException(nameof(arrayIndex));
        }

        if (arrayIndex + length > destination.Length) {
            throw new ArgumentException("There is not enouph length of destination array");
        }

        var obj_size = sizeof(T);
        fixed (T* arrayPtr = destination) {
            var byteLen = (long)(length * obj_size);
            var dest = new IntPtr(arrayPtr) + arrayIndex * obj_size;
            System.Buffer.MemoryCopy((void*)data, (void*)dest, byteLen, byteLen);
        }
    }

    /// <summary>Copy fron <see cref="NativeArray{T}"/>.</summary>
    /// <param name="source">source array of type <see cref="NativeArray{T}"/></param>
    public void copy_from(NativeArray<T> source) => copy_from(source.get_pointer(), 0, source.length);

    public void copy_from<S>(in S[] source) where S: unmanaged {
        var source_len = (long)(source.Length * sizeof(S));
        fixed (S* ptr = source) {
            System.Buffer.MemoryCopy(ptr, (void*)this.data, this.length * sizeof(T), source_len);
        }
    }

    /// <summary>Copy from <see cref="ReadOnlySpan{T}"/> to this <see cref="NativeArray{T}"/> of index 0.</summary>
    /// <param name="span"><see cref="ReadOnlySpan{T}"/> object.</param>
    public void copy_from(ReadOnlySpan<T> span) => copy_from(span, 0);

    /// <summary>Copy from <see cref="ReadOnlySpan{T}"/> to this <see cref="NativeArray{T}"/> of specified index.</summary>
    /// <param name="source"><see cref="ReadOnlySpan{T}"/> object.</param>
    /// <param name="start">start index of destination. (destination is this <see cref="NativeArray{T}"/>.)</param>
    public void copy_from(ReadOnlySpan<T> source, int start) {
        if (is_disposed()) Error.already_disposed(this);
        if (start < 0) {
            throw new ArgumentOutOfRangeException();
        }

        if (start + source.Length > length) {
            throw new ArgumentOutOfRangeException();
        }

        var obj_size = sizeof(T);
        fixed (T* ptr = source) {
            var byteLen = (long)(source.Length * obj_size);
            System.Buffer.MemoryCopy(ptr, (void*)(this.data + start * obj_size), byteLen, byteLen);
        }
    }

    /// <summary>Copy from unmanaged.</summary>
    /// <param name="source">unmanaged source pointer</param>
    /// <param name="start">start index of destination. (destination is this <see cref="NativeArray{T}"/>.)</param>
    /// <param name="length">count of copied item. (NOT length of bytes.)</param>
    public void copy_from(IntPtr source, int start, int length) {
        if (is_disposed()) Error.already_disposed(this);
        if (length == 0) {
            return;
        }

        if (source == IntPtr.Zero) {
            throw new ArgumentNullException(nameof(source), "source is null");
        }

        if (start < 0 || length < 0) {
            throw new ArgumentOutOfRangeException();
        }

        if (start + length > this.length) {
            throw new ArgumentOutOfRangeException();
        }

        var obj_size = sizeof(T);
        var byteLen = (long)(length * obj_size);
        System.Buffer.MemoryCopy((void*)source, (void*)(this.data + start * obj_size), byteLen, byteLen);
    }

    /// <summary>Return <see cref="Span{T}"/> of this <see cref="NativeArray{T}"/>.</summary>
    /// <returns><see cref="Span{T}"/></returns>
    public Span<T> as_span() {
        if (this.length == 0) {
            Error.index_out_of_range(0);
        }

        return new Span<T>((T*)this.data, this.length);
    }

    /// <summary>Return <see cref="Span{T}"/> starts with specified index.</summary>
    /// <param name="start">start index</param>
    /// <returns><see cref="Span{T}"/></returns>
    public Span<T> as_span(int start) {
        if (this.length == 0) {
            Error.index_out_of_range(0);
        }

        if ((uint)start > (uint)this.length) {
            Error.index_out_of_range(nameof(start), start);
        }

        return new Span<T>((T*)this.data + start, this.length - start);
    }

    /// <summary>Return <see cref="Span{T}"/> of specified length starts with specified index.</summary>
    /// <param name="start">start index</param>
    /// <param name="length">length of span</param>
    /// <returns><see cref="Span{T}"/></returns>
    public Span<T> as_span(int start, int length) {
        if (this.length == 0) {
            Error.index_out_of_range(0);
        }

        if ((uint)start > (uint)this.length) {
            Error.index_out_of_range(nameof(start), start + length);
        }

        if ((uint)length > (uint)this.length - (uint)start) {
            Error.index_out_of_range(nameof(length), start + length);
        }

        return new Span<T>((T*)this.data + start, length);
    }

    /// <summary>
    /// Dispose this instance and release unmanaged memory.<para/>
    /// If already disposed, do nothing.<para/>
    /// </summary>
    public void Dispose() {
        GC.SuppressFinalize(this);
        clear();
    }

    private void clear() {
        if (this.data == 0) return;
        NativeMemory.AlignedFree((void*)data);
        this.data = 0;
        this.length = 0;
    }

    /// <summary>Enumerator of <see cref="NativeArray{T}"/></summary>
    private sealed class Enumerator: IEnumerator<T> {
        private T* _ptr;
        private readonly nint _start;
        private readonly nint _end;

        /// <summary>Get current element</summary>
        public T Current {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => *_ptr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Enumerator(in NativeArray<T> array) {
            _start = array.get_pointer();
            _end = _start + array.length * sizeof(T);
            _ptr = (T*)(_start - sizeof(T)) - 1;
        }

        /// <summary>Dispose of <see cref="IDisposable"/></summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() {}

        /// <summary>Move to next element</summary>
        /// <returns>true if success to move next. false to end.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext() {
            if ((nuint)_ptr < (nuint)_end) {
                ++_ptr;
                return true;
            }

            return false;
        }

        object IEnumerator.Current => throw new NotImplementedException();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IEnumerator.Reset() => _ptr = (T*)(_start - sizeof(T)) - 1;
    }
}

internal class NativeArrayDebuggerTypeProxy<T> where T : unmanaged {
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly NativeArray<T> _entity;

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public T[] Items {
        get {
            var items = new T[_entity.length];
            _entity.copy_to(items, 0);
            return items;
        }
    }

    public NativeArrayDebuggerTypeProxy(NativeArray<T> entity) => _entity = entity;
}