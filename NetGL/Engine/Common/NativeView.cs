using System.Numerics;

namespace NetGL;

using System.Runtime.CompilerServices;

public readonly ref struct RefValue<T> where T : unmanaged, INumberBase<T> {
    private readonly ref T _value;

    internal RefValue(ref T value) => _value = ref value;

    public void set(T value) => _value = value;

    public void set<V>(in V value) where V : unmanaged, INumberBase<V> {
        _value = (T)Convert.ChangeType(value, typeof(T));;
    }

    public T get() => _value;

    public V get<V>() where V: unmanaged, INumberBase<V> {
        switch (_value) {
            case float number:
                return V.CreateSaturating(number);
            case Half number:
                return V.CreateSaturating(number);
            case double number:
                return V.CreateSaturating(number);

            case int number:
                return V.CreateSaturating(number);
            case short number:
                return V.CreateSaturating(number);

            case uint number:
                return V.CreateSaturating(number);
            case ushort number:
                return V.CreateSaturating(number);
            case byte number:
                return V.CreateSaturating(number);

            default:
                Error.type_conversion_error<T, V>(_value);
                return default;
        }
    }
}

public abstract class NativeView {
    public readonly int length = 0;
    public abstract void set<V>(int index, in V value) where V : unmanaged, INumberBase<V>;
    public abstract V get<V>(int index) where V : unmanaged, INumberBase<V>;

    protected NativeView(int length) { }
}

public unsafe class NativeView<T>: NativeView where T : unmanaged {
    private readonly nint start;
    private readonly nint stride;

    internal NativeView(nint start, nint stride, int length): base(length) {
        this.start = start;
        this.stride = stride;
    }

    public ref T this[int index] {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            if (index < 0 || start + index * stride >= length) Error.index_out_of_range(nameof(index), index);
            return ref *(T*)(start + index * stride);
        }
    }

    public override void set<V>(int index, in V value) {
        this[index] = (T)Convert.ChangeType(value, typeof(T));;
    }

    public override V get<V>(int index) {
        var value = this[index];

        switch (value) {
            case float number:
                return V.CreateSaturating(number);
            case Half number:
                return V.CreateSaturating(number);
            case double number:
                return V.CreateSaturating(number);

            case int number:
                return V.CreateSaturating(number);
            case short number:
                return V.CreateSaturating(number);

            case uint number:
                return V.CreateSaturating(number);
            case ushort number:
                return V.CreateSaturating(number);
            case byte number:
                return V.CreateSaturating(number);

            default:
                Error.type_conversion_error<T, V>(value);
                return default;
        }
    }
}