namespace NetGL;

using System.Collections;
using System.Runtime.CompilerServices;

public sealed class ArrayView<V>: IEnumerable<V> where V : unmanaged {
    public readonly int length;
    private readonly nint start;
    private readonly nint stride;

    internal ArrayView(nint start, nint stride, int length) {
        this.length = length;
        this.start  = start;
        this.stride = stride;
    }

    internal ArrayView(ArrayView<V> view) {
        this.length = view.length;
        this.start  = view.start;
        this.stride = view.stride;
    }

    public unsafe ref V this[int index] {
        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            if ((uint)index >= (uint)length) Error.index_out_of_range(index, length);
            return ref *(V*)(start + index * stride);
        }
    }

    IEnumerator<V> IEnumerable<V>.GetEnumerator() {
        for (var i = 0; i < length; i++) {
            yield return this[i];
        }
    }

    public ArrayWriter<V> new_writer() => new ArrayWriter<V>(this);

    IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();

    public override string ToString() => $"{GetType().get_type_name()} (length={length:N0}, stride={stride:N0})";

    public unsafe V* get_pointer()
        => (V*)start;
}