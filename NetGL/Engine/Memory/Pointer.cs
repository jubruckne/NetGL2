using System.Numerics;

namespace NetGL;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;

public interface IPointer {
    nint address { get; }
    int size_of();
    Type type_of();
    bool is_valid();
}

public static class Pointer {
    public static event Action<IPointer>? on_allocate;
    public static event Action<IPointer>? on_release;

    public static int alive_count { get; private set; } = 0;

    internal static void raise_allocate(IPointer pointer) {
        ++alive_count;
        on_allocate?.Invoke(pointer);
    }

    internal static void raise_release(IPointer pointer) {
        --alive_count;
        on_release?.Invoke(pointer);
    }
}

public unsafe class ArrayPointer<T>: Pointer<T> where T: unmanaged {
    private NativeArray<T> array => (NativeArray<T>)owner!;

    public ArrayPointer(in NativeArray<T> array, in int index): base(array, (T*)index, no_dispose_needed) {}

    public override ref T value {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            if (ptr == nint.MaxValue)
                Error.not_allocated(this);

            return ref array.by_ref((int)ptr);
        }
    }
}

[SuppressMessage("ReSharper", "MemberInitializerValueIgnored")]
public unsafe class Pointer<T>:
    IPointer,
    IDisposable,
    IEquatable<Pointer<T>>,
    IEquatable<T>
    where T: unmanaged {

    public static int size_of => sizeof(T);
    public static Type type_of => typeof(T);

    protected nint ptr = nint.MaxValue;
    protected object? owner; // used to keep owner alive

    public nint address => ptr;
    int IPointer.size_of() => sizeof(T);
    Type IPointer.type_of() => typeof(T);

    public bool is_valid() {
        if (owner == null)
            return ptr != nint.MaxValue && ptr != 0;

        return ptr != nint.MaxValue;
    }

    public delegate void DisposeDelegate(nint ptr, in object? owner);

    protected readonly DisposeDelegate on_dispose;

    ~Pointer() {
        if (ptr == nint.MaxValue) return;
        Pointer.raise_release(this);
        on_dispose(ptr, owner);
        owner = null;
        ptr = nint.MaxValue;
    }

    public Pointer() {
        owner = null;
        ptr = (nint)NativeMemory.AllocZeroed(1, (nuint)sizeof(T));
        on_dispose = dispose_self_allocated;
        Pointer.raise_allocate(this);
    }

    public Pointer(in object owner, in T* data, in DisposeDelegate on_dispose) {
        this.owner      = owner;
        this.ptr        = (nint)data;
        this.on_dispose = on_dispose;
        Pointer.raise_allocate(this);
    }

    public Pointer(in T* data, in DisposeDelegate on_dispose) {
        owner = null;
        ptr             = (nint)data;
        this.on_dispose = on_dispose;
        Pointer.raise_allocate(this);
    }

    public Pointer(in Pointer<T> pointer) {
        this.owner      = pointer.owner;
        this.ptr        = pointer.ptr;
        this.on_dispose = no_dispose_needed;
        Pointer.raise_allocate(this);
    }

    public bool is_owned => owner != null;

    protected static void no_dispose_needed(nint this_ptr, in object? owner) {}

    private static void dispose_self_allocated(nint this_ptr, in object? owner)
        => NativeMemory.Free((void*)this_ptr);

    void IDisposable.Dispose() {
        if (ptr == nint.MaxValue) return;

        var temp_ptr = nint.MaxValue;

        temp_ptr = Interlocked.Exchange(ref this.ptr, temp_ptr);

        if (temp_ptr != nint.MaxValue) {
            Pointer.raise_release(this);

            var temp_owner = this.owner;
            owner = null;
            on_dispose(temp_ptr, temp_owner);
        }

        GC.SuppressFinalize(this);
    }

    public virtual ref T value {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            if (ptr == nint.MaxValue)
                Error.not_allocated(this);

            return ref *(T*)ptr;
        }
    }

    public override string ToString() {
        if (is_valid()) return $"{typeof(T).get_type_name()}* {value.ToString()}";
        return $"{typeof(T).get_type_name()}* null";
    }

    bool IEquatable<Pointer<T>>.Equals(Pointer<T>? other) => other is not null && other.ptr == ptr;

    public bool Equals(T other) {
        fixed (T* p1 = &value) {
            var o1 = new ReadOnlySpan<byte>((byte*)p1, sizeof(T));
            var o2 = new ReadOnlySpan<byte>((byte*)&other, sizeof(T));
            return o1.SequenceEqual(o2);
        }
    }

    public override bool Equals(object? obj) {
        return obj switch {
            Pointer<T> pt => this == pt,
            IntPtr ip     => this == ip,
            T t           => Equals(t),
            _             => false
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    public override int GetHashCode() => ptr.GetHashCode();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator==(Pointer<T> left, Pointer<T> right) => left.ptr == right.ptr;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator!=(Pointer<T> left, Pointer<T> right) => left.ptr != right.ptr;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator==(Pointer<T> left, IntPtr right) => left.ptr == right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator!=(Pointer<T> left, IntPtr right) => left.ptr != right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator T(in Pointer<T> p) => *(T*)p.ptr;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator T*(in Pointer<T> p) => (T*)p.ptr;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator IntPtr (in Pointer<T> p) => p.ptr;
}