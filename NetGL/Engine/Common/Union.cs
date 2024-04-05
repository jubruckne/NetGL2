#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace NetGL;

public readonly ref struct Union<T1, T2> {
    private readonly int tag;
    private readonly ref T1 t1;
    private readonly ref T2 t2;

    public Union(ref readonly T1 value) {
        tag = 0;
        t1 = value;
    }

    public Union(ref readonly T2 value) {
        tag = 1;
        t2 = value;
    }

    public T match<T>(Func<T1, T> case1, Func<T2, T> case2) {
        return tag switch {
            1 => case1(t1),
            2 => case2(t2),
            _ => throw new InvalidOperationException("Union not initialized properly.")
        };
    }
}