using System.Runtime.CompilerServices;

namespace NetGL;

public unsafe class Converter<TFrom, TTo> {
    private readonly delegate*<TTo, TFrom> convert_from;
    private readonly delegate*<TFrom, TTo> convert_to;

    public Converter(delegate*<TTo, TFrom> convert_from, delegate*<TFrom, TTo> convert_to) {
        this.convert_from = convert_from;
        this.convert_to = convert_to;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TTo to(TFrom from) => convert_to(from);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TFrom from(TTo from) => convert_from(from);
}