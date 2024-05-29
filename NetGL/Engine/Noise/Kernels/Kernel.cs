using System.Runtime.Intrinsics;

namespace NetGL;

public interface IKernel<T> where T: unmanaged {
    static abstract T evaluate(T x, T y);
}

public interface IKernel {
    static abstract Vector128<float> evaluate(Vector128<float> x, Vector128<float> y);
}