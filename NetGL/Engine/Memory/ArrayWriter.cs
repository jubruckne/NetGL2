namespace NetGL;

using System.Runtime.CompilerServices;

public sealed class ArrayWriter<V> where V: unmanaged {
    private readonly ArrayView<V> view;
    private readonly int length;
    private int pos;

    public int position {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => pos;
    }

    public bool eof => pos >= length;

    public int remaining => int.Max(0, length - pos);

    public void rewind() => pos = 0;

    public V value {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => view[pos];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => view[pos] = value;
    }

    internal ArrayWriter(in ArrayView<V> view) {
        this.view   = view;
        this.length = view.length;
        this.pos    = 0;
    }

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int write(in V value) {
        view[pos] = value;
        return pos++;
    }

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int write(in V value1, in V value2) {
        view[pos] = value1;
        view[pos + 1] = value2;
        return pos += 2;
    }

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int write(in V value1, in V value2, in V value3) {
        view[pos]     = value1;
        view[pos + 1] = value2;
        view[pos + 2] = value3;
        return pos += 3;
    }

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int write(in V value1, in V value2, in V value3, in V value4) {
        view[pos]     = value1;
        view[pos + 1] = value2;
        view[pos + 2] = value3;
        view[pos + 3] = value4;
        return pos += 4;
    }

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool advance() {
        if (pos < length) {
            ++pos;
            return true;
        }

        return false;
    }

    public override string ToString() => $"{this.get_type_name()} (view={view}, position={pos:N0}, remaining={remaining:N0})";
}