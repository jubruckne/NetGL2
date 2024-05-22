using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace NetGL;

public interface IKernel {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static abstract Vector128<float> evaluate(Vector128<float> x, Vector128<float> y);
}