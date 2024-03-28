using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NetGL;

public sealed unsafe class Field<T>: IEnumerable<T>, IDisposable where T: unmanaged {
    public int width { get; private set; }
    public int height { get; private set; }
    private nint data;

    public int total_size => width * height * sizeof(T);

    public Field(int width, int height, bool zero_out = true) {
        if (width < 0) Error.index_out_of_range(nameof(width), width);
        if (height < 0) Error.index_out_of_range(nameof(height), height);

        var bytes = sizeof(T) * (width * height);
        if (bytes < 0) {
            Error.index_out_of_range(nameof(T), bytes);
        }

        this.data = (IntPtr)NativeMemory.AlignedAlloc((UIntPtr)bytes, 64);
        this.width = width;
        this.height = height;

        if(zero_out) zero();
    }

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<T>)this).GetEnumerator();

    IEnumerator<T> IEnumerable<T>.GetEnumerator() {
        for (var x = 0; x < height; ++x) {
            for (var y = 0; y < width; ++y) {
                yield return this[x, y];
            }
        }
    }

    ~Field() => clear();

    /*
    IEnumerator<string> IEnumerable<string>.GetEnumerator() {
        for (var row = 0; row < height; ++row) {
            var list = new List<T>();

            for (var col = 0; col < width; ++col) {
                list.Add(this[col, row]);
            }

            yield return list.array_to_string();
        }
    }
*/
    // IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<string>)this).GetEnumerator();

    public IEnumerable<(int x, int y, T data)> this[Range rows, Range columns] {
        get {
            var row_range = rows.GetOffsetAndLength(height);
            var col_range = columns.GetOffsetAndLength(width);

            for (var row = row_range.Offset; row < row_range.Offset + row_range.Length; ++row) {
                for (var col = col_range.Offset; col < col_range.Offset + col_range.Length; ++col) {
                    yield return (col, row, this[col, row]);
                }
            }
        }
    }

    public T this[int x, int y] {
        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            if (x < 0 || x >= width) Error.index_out_of_range(x, width);
            if (y < 0 || y >= height) Error.index_out_of_range(y, height);
            return ((T*)data)[x + y * width];
        }

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set {
            if (x < 0 || x >= width) Error.index_out_of_range(x, width);
            if (y < 0 || y >= height) Error.index_out_of_range(y, height);
            ((T*)data)[x + y * width] = value;
        }
    }

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T by_ref(int x, int y) {
        if (x < 0 || x >= width) Error.index_out_of_range(x, width);
        if (y < 0 || y >= height) Error.index_out_of_range(y, height);
        return ref ((T*)data)[x + y * width];
    }

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref C by_ref<C>(int x, int y) where C: unmanaged {
        if (x < 0 || x >= width) Error.index_out_of_range(x, width);
        if (y < 0 || y >= height) Error.index_out_of_range(y, height);
        if (sizeof(T) != sizeof(C)) Error.type_conversion_error<T, C>(this[x, y]);
        return ref *(C*)&((T*)data)[x + y * width];
    }

    public void zero() => as_span().Clear();
    public void fill(in T value) => as_span().Fill(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool is_disposed() => data == IntPtr.Zero;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public nint get_address(int x, int y) {
        if (x < 0 || x >= width) Error.index_out_of_range(x, width);
        if (y < 0 || y >= height) Error.index_out_of_range(y, height);

        if (!is_disposed()) return data + (x + y * width) * sizeof(T);

        Error.already_disposed(this);
        return 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public nint get_address() {
        if (width == 0 || height == 0) Error.index_out_of_range(0);

        if (!is_disposed()) return data;

        Error.already_disposed(this);
        return 0;
    }

    public Span<T> as_span() {
        if (width == 0 || height == 0) Error.index_out_of_range(0);
        return new ((T*)data, width * height);
    }

    public Span<T> as_span(int row) {
        if (width == 0 || height == 0) Error.index_out_of_range(0);
        return new ((T*)(data + row * width * sizeof(T)), width * sizeof(T));
    }


    public Span<V> as_span<V>() where V : unmanaged {
        if (width == 0 || height == 0) Error.index_out_of_range(0);
        return new((T*)data, (width * height) * sizeof(T) / sizeof(V));
    }

    public ReadOnlySpan<V> as_readonly_span<V>() where V : unmanaged {
        if (width == 0 || height == 0) Error.index_out_of_range(0);
        return new((T*)data, (width * height) * sizeof(T) / sizeof(V));
    }

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

        data = 0;
        width = 0;
        height = 0;
    }

    public override string ToString() => $"{this.get_type_name()}[{width}, {height}]";
}