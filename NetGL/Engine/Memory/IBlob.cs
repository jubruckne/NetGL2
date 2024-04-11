/*using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NetGL;

public unsafe class Blob<T>: IDisposable where T: unmanaged {
    private nint data;
    public readonly int length;

    ~Blob() {
        if (data != 0) Dispose();
    }

    protected Blob(int length) {
        data = (int)NativeMemory.AlignedAlloc((UIntPtr)(len  * Unsafe.SizeOf<T>()), 16);
        this.length = length;
    }

    public void Dispose() {
        if (data != 0) return;
        var ptr = Interlocked.Exchange(ref data, 0);
        if(ptr != 0)
            NativeMemory.AlignedFree((void*)data);
        GC.SuppressFinalize(this);
    }

    public static Blob<T> allocate(int length) => new(length);
}

public sealed class View2D<T>: IDisposable where T: unmanaged {
    private readonly Blob<T> blob;

    public int width { get; }
    public int height { get; }

    public View2D(int width, int height, IBlob blob) {
        if(blob.size != width * height * item_size)
            Error.exception($"blob.size != width * height * item_size");

        this.blob = blob;
        this.width = width;
        this.height = height;
        this.length = width * height;
    }

    public unsafe T this[int x, int y] {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            var index = x + y * width;
            if ((uint)index >= (uint)length) Error.index_out_of_range(index, length);
            return ((T*)blob.base_address)[index];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set {
            var index = x + y * width;
            if ((uint)index >= (uint)length) Error.index_out_of_range(index, length);
            ((T*)blob.base_address)[index] = value;
        }
    }

    public void Dispose() {
        blob.Dispose();
    }
}

public sealed class View<T>: Blob<T> where T: unmanaged {
    private static readonly int item_size = Unsafe.SizeOf<T>();

    private int start;

    public View(int length): base(length) {
    }

    public unsafe T this[int index] {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            if ((uint)index >= (uint)length) Error.index_out_of_range(index, length);
            return ((T*)blob.base_address)[index];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set {
            if ((uint)index >= (uint)length) Error.index_out_of_range(index, length);
            ((T*)blob.base_address)[index] = value;
        }
    }

    public View<T> this[Range range] {
        get {
            var (start, length) = range.GetOffsetAndLength(this.length);
            if ((uint)start >= (uint)this.length) Error.index_out_of_range(start, this.length);
            if ((uint)start + (uint)length >= (uint)this.length) Error.index_out_of_range(length, this.length - start);
            return new(length, SlicedBlob.slice(blob, [start / item_size], [length / item_size]));
        }
    }

    public unsafe Span<T> as_span() {
        if (length == 0) Error.index_out_of_range(0);
        return new((T*)blob.base_address, length);
    }

    public void Dispose() {
        blob.Dispose();
    }
}

public static class View {
    public static View<T> allocate<T>(int length) where T: unmanaged
        => new View<T>(length, AllocatingBlob.allocate(length * Unsafe.SizeOf<T>()));

    public static View2D<T> allocate<T>(int width, int height) where T: unmanaged
        => new View2D<T>(width, height, AllocatingBlob.allocate(width * height * Unsafe.SizeOf<T>()));
}

public static class testttt {
    public static void hhhhh() {
        var data = View.allocate<float>(100, 100);

        data[0, 0]++;

        data.Dispose();



    }
}*/