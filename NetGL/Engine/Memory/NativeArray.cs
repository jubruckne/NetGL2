namespace NetGL;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

[SkipLocalsInit]
public unsafe class NativeArray<T>: IEnumerable<T>, IDisposable where T : unmanaged {
    public int length { get; private set; }
    private nint data;

    public int total_size => length * sizeof(T);

    public NativeArray(int length, bool zero_out = true) {
        if (length < 0) {
            Error.index_out_of_range(length);
        }

        var bytes = sizeof(T) * length;
        if (bytes < 0) {
            Error.index_out_of_range(length);
        }

        this.data = (IntPtr)NativeMemory.AlignedAlloc((UIntPtr)bytes, 16);
        this.length = length;
        if(zero_out) zero();
    }

    /// <summary>Create new <see cref="NativeArray{T}"/>, those elements are copied from <see cref="ReadOnlySpan{T}"/>.</summary>
    /// <param name="span">Elements of the <see cref="NativeArray{T}"/> are initialized by this <see cref="ReadOnlySpan{T}"/>.</param>
    public NativeArray(ReadOnlySpan<T> span) : this(span.Length) {
        span.CopyTo(this.as_span());
    }

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T by_ref(int index) {
        if (index < 0 || index >= length) Error.index_out_of_range(index);
        return ref ((T*)data)[index];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref C by_ref<C>(int index) where C: unmanaged {
        if (sizeof(T) != sizeof(C)) Error.type_conversion_error<T, C>(this[index]);
        if (index < 0 || index >= length) Error.index_out_of_range(index);
        return ref *(C*)get_address(index);
    }

    public void zero() => as_span().Clear();
    public void fill(in T value) => as_span().Fill(value);

    public ArrayView<T> get_view() {
        if (length == 0) Error.index_out_of_range(0);
        return new(
                   data,
                   sizeof(T),
                   length * sizeof(T)
                  );
    }

    public ArrayView<T> get_view(Range range) {
        if (length == 0) Error.index_out_of_range(0);

        var range_data = range.GetOffsetAndLength(length);

        return new(
                   data + range_data.Offset * sizeof(T),
                   sizeof(T),
                   range_data.Length * sizeof(T)
                  );
    }

    public ArrayView<V> get_view<V>() where V : unmanaged {
        if (length == 0) Error.index_out_of_range(0);

        return new(
            data,
            sizeof(V),
            length * sizeof(T) / sizeof(V)
        );
    }

    public ArrayView<V> get_view<V>(Range range) where V : unmanaged {
        if (length == 0) Error.index_out_of_range(0);

        var range_data = range.GetOffsetAndLength(length);

        return new(
                   data + range_data.Offset * sizeof(T) / sizeof(V),
                   sizeof(V),
                   range_data.Length * sizeof(T) / sizeof(V)
                  );
    }

    internal ArrayView<V> get_view<V>(nint element_offset) where V : unmanaged {
        if (element_offset < 0 || element_offset > sizeof(T)) Error.index_out_of_range(nameof(element_offset), element_offset);
        if (length == 0) Error.index_out_of_range(0);

        return new(
            data + element_offset,
            sizeof(T),
            length
        );
    }

    internal ArrayView<V> get_view<V>(in string field_name) where V : unmanaged {
        if (length == 0) Error.index_out_of_range(0);

        return new(
            data + Marshal.OffsetOf<T>(field_name),
            sizeof(T),
            length
        );
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
    }

    ~NativeArray() => clear();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool is_disposed() => data == IntPtr.Zero;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public nint get_address(int index) {
        if (length == 0) Error.index_out_of_range(0);
        if (index < 0 || index >= length) Error.index_out_of_range(index);

        if (!is_disposed()) return data + sizeof(T) * index;

        Error.already_disposed(this);
        return 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public nint get_address() {
        if (length == 0) Error.index_out_of_range(0);

        if (!is_disposed()) return data;

        Error.already_disposed(this);
        return 0;
    }


    IEnumerator<T> IEnumerable<T>.GetEnumerator() {
        if (is_disposed()) Error.already_disposed(this);

        for (var index = 0; index < length; index++) {
            yield return this[index];
        }
    }

    IEnumerator IEnumerable.GetEnumerator() {
        if (is_disposed()) Error.already_disposed(this);

        for (var index = 0; index < length; index++) {
            yield return this[index];
        }
    }

    public void insert(ReadOnlySpan<T> items, int at = 0) {
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
    public void copy_from(NativeArray<T> source) => copy_from(source.get_address(), 0, source.length);

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

    public Span<V> as_span<V>() where V : unmanaged {
        if (length == 0) Error.index_out_of_range(0);
        return new Span<V>((T*)this.data, this.length * sizeof(T) / sizeof(V));
    }

    public Span<T> as_span() {
        if (length == 0) Error.index_out_of_range(0);
        return new Span<T>((T*)this.data, this.length);
    }

    public Span<T> as_span(int start) {
        if (length == 0) Error.index_out_of_range(0);

        if ((uint)start > (uint)this.length) {
            Error.index_out_of_range(nameof(start), start);
        }

        return new Span<T>((T*)this.data + start, this.length - start);
    }

    public Span<T> as_span(int start, int length) {
        if (length == 0) Error.index_out_of_range(0);

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
        Console.WriteLine("NativeArray.Dispose");
        Console.WriteLine("NativeArray.Dispose");
        Console.WriteLine("NativeArray.Dispose");
        GC.SuppressFinalize(this);
        clear();
    }

    private void clear() {
        Console.WriteLine("NativeArray.Clear");
        Console.WriteLine("NativeArray.Clear");
        Console.WriteLine("NativeArray.Clear");
        Console.WriteLine("NativeArray.Clear");
        if (data == 0) return;
        NativeMemory.AlignedFree((void*)data);

        this.data = 0;
        this.length = 0;
    }
}