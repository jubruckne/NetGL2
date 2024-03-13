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
public sealed unsafe class NativeArray<T>: IEnumerable<T>, IDisposable where T : unmanaged {
    public int rows { get; private set; }
    public int columns { get; private set; }
    public int length { get; private set; }

    private IntPtr array;

    /// <summary>Get pointer address of this array.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IntPtr get_pointer() => array;

    /// <summary>Get the specific item of specific index.</summary>
    /// <param name="i">index</param>
    /// <returns>The item of specific index</returns>
    public T this[int i] {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => get_reference(i);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => get_reference(i) = value;
    }

    /// <summary>Get the specific item of specific index.</summary>
    /// <param name="row">row</param>
    /// <param name="col">column</param>
    /// <returns>The item of specific index</returns>
    public T this[int col, int row] {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => get_reference(row * columns + col);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => get_reference(row * columns + col) = value;
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
        if(length < 0) {
            Error.index_out_of_range(nameof(length), length);
        }
        var bytes = sizeof(T) * length;
        if(bytes < 0) {
            Error.index_out_of_range(nameof(T), bytes);
        }
        this.array = Marshal.AllocHGlobal(bytes);
        this.length = length;
        this.rows = length;
        this.columns = 1;
    }

    /// <summary>Create new <see cref="NativeArray{T}"/>, those elements are copied from <see cref="ReadOnlySpan{T}"/>.</summary>
    /// <param name="span">Elements of the <see cref="NativeArray{T}"/> are initialized by this <see cref="ReadOnlySpan{T}"/>.</param>
    public NativeArray(ReadOnlySpan<T> span) {
        var bytes = span.Length * sizeof(T);
        if(bytes == 0) { return; }
        this.array = Marshal.AllocHGlobal(bytes);
        this.length = span.Length;
        span.CopyTo(new Span<T>((void*)array, length));
    }

    /// <summary>Finalizer of <see cref="NativeArray{T}"/></summary>
    ~NativeArray() => Dispose(false);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool is_disposed() => array == IntPtr.Zero;

    /// <summary>Get reference to head item (Returns ref to null if empty)</summary>
    /// <returns>reference to head item</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T get_reference() {
        if(length == 0) {
            Error.index_out_of_range(0);
        }
        return ref Unsafe.AsRef<T>((T*)array);
    }

    /// <summary>Get reference to head item (Returns ref to null if empty)</summary>
    /// <returns>reference to head item</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T get_reference(int index) {
        if((uint)index >= (uint)length) {
            Error.index_out_of_range(index);
        }
        return ref Unsafe.Add(ref get_reference(), index);
    }

    /// <summary>Get reference to head item (Returns ref to null if empty)</summary>
    /// <returns>reference to head item</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T get_reference(int column, int row) {
        if(column >= this.columns) Error.index_out_of_range(nameof(column), column);
        if(row >= this.rows) Error.index_out_of_range(nameof(row), row);
        return ref get_reference(row * columns + column);
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator() {
        if(is_disposed()) Error.already_disposed(this);
        // Avoid boxing by using class enumerator.
        return new EnumeratorClass(this);
    }

    IEnumerator IEnumerable.GetEnumerator() {
        if(is_disposed()) Error.already_disposed(this);
        // Avoid boxing by using class enumerator.
        return new EnumeratorClass(this);
    }

    /// <summary>Copy to managed memory</summary>
    /// <param name="destination">managed memory array</param>
    /// <param name="arrayIndex">start index of destination array</param>
    public void copy_to(T[] destination, int arrayIndex) {
        if(is_disposed()) Error.already_disposed(this);
        if((uint)arrayIndex >= (uint)destination.Length) { throw new ArgumentOutOfRangeException(nameof(arrayIndex)); }
        if(arrayIndex + length > destination.Length) { throw new ArgumentException("There is not enouph length of destination array"); }

        var obj_size = sizeof(T);
        fixed(T* arrayPtr = destination) {
            var byteLen = (long)(length * obj_size);
            var dest = new IntPtr(arrayPtr) + arrayIndex * obj_size;
            System.Buffer.MemoryCopy((void*)array, (void*)dest, byteLen, byteLen);
        }
    }

    /// <summary>Copy fron <see cref="NativeArray{T}"/>.</summary>
    /// <param name="source">source array of type <see cref="NativeArray{T}"/></param>
    public void copy_from(NativeArray<T> source) => copy_from(source.get_pointer(), 0, source.length);

    /// <summary>Copy from <see cref="ReadOnlySpan{T}"/> to this <see cref="NativeArray{T}"/> of index 0.</summary>
    /// <param name="span"><see cref="ReadOnlySpan{T}"/> object.</param>
    public void copy_from(ReadOnlySpan<T> span) => copy_from(span, 0);

    /// <summary>Copy from <see cref="ReadOnlySpan{T}"/> to this <see cref="NativeArray{T}"/> of specified index.</summary>
    /// <param name="source"><see cref="ReadOnlySpan{T}"/> object.</param>
    /// <param name="start">start index of destination. (destination is this <see cref="NativeArray{T}"/>.)</param>
    public void copy_from(ReadOnlySpan<T> source, int start) {
        if(is_disposed()) Error.already_disposed(this);
        if(start < 0) { throw new ArgumentOutOfRangeException(); }
        if(start + source.Length > length) { throw new ArgumentOutOfRangeException(); }
        var obj_size = sizeof(T);
        fixed(T* ptr = source) {
            var byteLen = (long)(source.Length * obj_size);
            System.Buffer.MemoryCopy(ptr, (void*)(this.array + start * obj_size), byteLen, byteLen);
        }
    }

    /// <summary>Copy from unmanaged.</summary>
    /// <param name="source">unmanaged source pointer</param>
    /// <param name="start">start index of destination. (destination is this <see cref="NativeArray{T}"/>.)</param>
    /// <param name="length">count of copied item. (NOT length of bytes.)</param>
    public void copy_from(IntPtr source, int start, int length) {
        if(is_disposed()) Error.already_disposed(this);
        if(length == 0) { return; }
        if(source == IntPtr.Zero) { throw new ArgumentNullException(nameof(source),"source is null"); }
        if(start < 0 || length < 0) { throw new ArgumentOutOfRangeException(); }
        if(start + length > this.length) { throw new ArgumentOutOfRangeException(); }
        var obj_size = sizeof(T);
        var byteLen = (long)(length * obj_size);
        System.Buffer.MemoryCopy((void*)source, (void*)(this.array + start * obj_size), byteLen, byteLen);
    }

    /// <summary>Return <see cref="Span{T}"/> of this <see cref="NativeArray{T}"/>.</summary>
    /// <returns><see cref="Span{T}"/></returns>
    public Span<T> as_span() {
        if(this.length == 0) {
            Error.index_out_of_range(0);
        }

        return new Span<T>((T*)this.array, this.length);
    }

    /// <summary>Return <see cref="Span{T}"/> starts with specified index.</summary>
    /// <param name="start">start index</param>
    /// <returns><see cref="Span{T}"/></returns>
    public Span<T> as_span(int start) {
        if(this.length == 0) {
            Error.index_out_of_range(0);
        }

        if((uint)start > (uint)this.length) {
            Error.index_out_of_range(nameof(start), start);
        }

        return new Span<T>((T*)this.array + start, this.length - start);
    }

    /// <summary>Return <see cref="Span{T}"/> of specified length starts with specified index.</summary>
    /// <param name="start">start index</param>
    /// <param name="length">length of span</param>
    /// <returns><see cref="Span{T}"/></returns>
    public Span<T> as_span(int start, int length) {
        if(this.length == 0) {
            Error.index_out_of_range(0);
        }
        if((uint)start > (uint)this.length) {
            Error.index_out_of_range(nameof(start), start + length);
        }

        if((uint)length > (uint)this.length - (uint)start) {
            Error.index_out_of_range(nameof(length), start + length);
        }

        return new Span<T>((T*)this.array + start, length);
    }

    /// <summary>
    /// Dispose this instance and release unmanaged memory.<para/>
    /// If already disposed, do nothing.<para/>
    /// </summary>
    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing) {
        if(this.array == IntPtr.Zero) { return; }
        Marshal.FreeHGlobal(this.array);
        this.array = IntPtr.Zero;
        this.length = 0;
    }

    /// <summary>Enumerator of <see cref="NativeArray{T}"/></summary>
    private class EnumeratorClass: IEnumerator<T> {
        private readonly T* _ptr;
        private readonly int _len;
        private int _index;

        /// <summary>Get current element</summary>
        public T Current { get; private set; }

        internal EnumeratorClass(NativeArray<T> array) {
            _ptr = (T*)array.array;
            _len = array.length;
            _index = 0;
            Current = default;
        }

        /// <summary>Dispose of <see cref="IDisposable"/></summary>
        public void Dispose() { }

        /// <summary>Move to next element</summary>
        /// <returns>true if success to move next. false to end.</returns>
        public bool MoveNext() {
            if((uint)_index < (uint)_len) {
                Current = _ptr[_index];
                _index++;
                return true;
            }
            return MoveNextRare();
        }

        private bool MoveNextRare() {
            _index = _len + 1;
            Current = default;
            return false;
        }

        object IEnumerator.Current => Current;

        void IEnumerator.Reset() {
            _index = 0;
            Current = default;
        }
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