using System.Runtime.CompilerServices;

namespace NetGL;

public readonly unsafe ref struct Pointer<T> where T: unmanaged {
    private readonly T* variable;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Pointer(T* variable) => this.variable = variable;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref C cast<C>() where C: unmanaged => ref *(C*)variable;
}

public readonly unsafe ref struct Pointer<T, C> where T: unmanaged where C: unmanaged {
    private readonly T* variable;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Pointer(T* variable) => this.variable = variable;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref C cast() => ref *(C*)variable;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator C(in Pointer<T, C> p) => *(C*)p.variable;
}