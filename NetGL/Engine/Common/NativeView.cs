using System.Numerics;

namespace NetGL;

using System.Runtime.CompilerServices;

public static class NativeView {
    public readonly unsafe struct Structs<T> where T: unmanaged {
        private readonly nint start;
        private readonly nint stride;
        private readonly nint buffer_size;

        public readonly int length = 0;

        internal Structs(nint start, nint stride, int length, nint buffer_size) {
            this.length = length;
            this.start = start;
            this.stride = stride;
            this.buffer_size = buffer_size;
        }

        public ref T this[int index] {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                if (index < 0 || start + index * stride >= length) Error.index_out_of_range(nameof(index), index);
                return ref Unsafe.AsRef<T>((void*)(start + index * stride));
            }
        }

        public Structs<V> as_structs<V>() where V : unmanaged {
            return new Structs<V>(start, sizeof(V), (int)(buffer_size / sizeof(V)), buffer_size);
        }

        public Numbers<V> as_numbers<V>() where V : unmanaged, INumberBase<V> {
            return new Numbers<V>(start, sizeof(V), (int)(buffer_size / sizeof(V)), buffer_size);
        }
    }

    public readonly unsafe struct Numbers<T> where T : unmanaged, INumberBase<T> {
        private readonly nint start;
        private readonly nint stride;
        private readonly nint buffer_size;

        public readonly int length = 0;

        internal Numbers(nint start, nint stride, int length, nint buffer_size) {
            this.length = length;
            this.start = start;
            this.stride = stride;
            this.buffer_size = buffer_size;
        }

        public ref T this[int index] {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                if (index < 0 || start + index * stride >= length) Error.index_out_of_range(nameof(index), index);
                return ref Unsafe.AsRef<T>((void*)(start + index * stride));
            }
        }

        public Structs<V> as_struct<V>() where V : unmanaged {
            return new Structs<V>(start, sizeof(V), (int)(buffer_size / sizeof(V)), buffer_size);
        }

        public Numbers<V> as_number<V>() where V : unmanaged, INumberBase<V> {
            return new Numbers<V>(start, sizeof(V), (int)(buffer_size / sizeof(V)), buffer_size);
        }

        public Converter<T, U> translate_to<U>()
            where U : unmanaged, INumberBase<U> {
            return new Converter<T, U>(start, sizeof(T), (int)(buffer_size / sizeof(T)), buffer_size);
        }
    }

    public readonly unsafe struct Converter<TFrom, TTo>
        where TFrom : unmanaged, INumberBase<TFrom>
        where TTo : unmanaged, INumberBase<TTo> {

        private readonly nint start;
        private readonly nint stride;
        private readonly nint buffer_size;

        public readonly int length = 0;

        internal Converter(nint start, nint stride, int length, nint buffer_size) {
            this.length = length;
            this.start = start;
            this.stride = stride;
            this.buffer_size = buffer_size;
        }

        public TTo this[int index] {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                if (index < 0 || start + index * stride >= length) Error.index_out_of_range(nameof(index), index);
                var v = Unsafe.AsRef<TFrom>((void*)(start + index * stride));
                return TTo.CreateSaturating(v);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set {
                if (index < 0 || start + index * stride >= length) Error.index_out_of_range(nameof(index), index);
                Unsafe.AsRef<TFrom>((void*)(start + index * stride)) = TFrom.CreateSaturating(value);
            }
        }
    }
}