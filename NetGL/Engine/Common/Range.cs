using System.Numerics;

namespace NetGL;

public readonly struct Range<T> where T: INumber<T> {
    private readonly T min;
    private readonly T max;

    public Range(T min, T max) {
        this.min = min;
        this.max = max;
    }

    public T clamp(in T value) => T.Clamp(value, min, max);
}