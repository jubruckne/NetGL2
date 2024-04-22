using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NetGL;

public readonly struct HashCode32 {
    private readonly uint hash;

    public static readonly HashCode32 zero = new(0);

    private HashCode32(uint hash)
        => this.hash = hash;

    public static HashCode32 of<T>(T data) where T: unmanaged
        => new(Murmur.hash32(typeof(T).FullName, (uint)Unsafe.SizeOf<T>()) ^ Murmur.hash32(data));

    public static HashCode32 of(string str)
        => new(Murmur.hash32(typeof(string).FullName, (uint)str.Length) ^ Murmur.hash32(str));

    public static HashCode32 of<T>(ReadOnlySpan<T> data)
        where T: unmanaged
        => new(Murmur.hash32(typeof(T[]).FullName, (uint)data.Length) ^ Murmur.hash32(MemoryMarshal.AsBytes(data)));

    public static bool operator ==(HashCode32 left, HashCode32 right) => left.hash == right.hash;
    public static bool operator !=(HashCode32 left, HashCode32 right) => left.hash != right.hash;

    public override string ToString() {
        var high = hash >> 16;
        var low  = hash & 0xFFFF;
        return $"{high:X}-{low:X}";
    }
}

public readonly struct HashCode64 {
    private readonly ulong hash;

    public static readonly HashCode64 zero = new(0);

    private HashCode64(ulong hash)
        => this.hash = hash;

    public static HashCode64 of(string str) {
        var low  = Murmur.hash32(typeof(string).FullName);
        var high = Murmur.hash32(str);
        return new(((ulong)low << 32) | high);
    }

    public static HashCode64 of<T>(T data)
        where T: unmanaged {
        var low = Murmur.hash32(typeof(T).FullName);
        var high = Murmur.hash32(data);
        return new(((ulong)low << 32) | high);
    }

    public static HashCode64 of<T>(ReadOnlySpan<T> data)
        where T: unmanaged {
        var low  = Murmur.hash32(typeof(T[]).FullName, (uint)data.Length);
        var high = Murmur.hash32(MemoryMarshal.AsBytes(data));
        return new(((ulong)low << 32) | high);
    }

    public static bool operator ==(HashCode64 left, HashCode64 right) => left.hash == right.hash;
    public static bool operator !=(HashCode64 left, HashCode64 right) => left.hash != right.hash;

    public override string ToString() {
        var high = (uint)(hash >> 32);
        var low  = (uint)(hash & 0xFFFFFFFF);
        return $"{high:X}-{low:X}";
    }
}

file static class Murmur {
    private const uint default_seed = 4203733937;

    public static uint type_hash<T>(ref readonly T data) where T: notnull {
        string type_name = "";

        if (data is string str)
            type_name = $"{typeof(T).FullName}[{str.Length}]";
        else if (data is Array arr)
            type_name = $"{typeof(T).FullName}[{arr.Length}]";
        else if (typeof(T).IsArray)
            type_name = $"{typeof(T).FullName}[{Unsafe.SizeOf<T>()}]";

        return hash32(type_name);
        //   return hash32(name, (uint)str.Length) ^ hash32(str);
    }

    public static uint hash32<T>(T data, uint seed = default_seed)
        where T: unmanaged {
        var span = MemoryMarshal.CreateReadOnlySpan(ref data, 1);
        return hash32(MemoryMarshal.AsBytes(span), seed);
    }

    public static uint hash32(ReadOnlySpan<char> bytes, uint seed = default_seed)
        => hash32(MemoryMarshal.AsBytes(bytes), seed);

    public static uint hash32(ReadOnlySpan<byte> bytes, uint seed = default_seed) {
        var length    = bytes.Length;
        var h1 = seed;
        var remainder = length & 3;
        var position  = length - remainder;
        for (var start = 0; start < position; start += 4)
            h1 = (uint)((int)RotateLeft(
                                        h1 ^ (RotateLeft(BitConverter.ToUInt32(bytes.Slice(start, 4)) * 3432918353U, 15)
                                              * 461845907U),
                                        13
                                       ) * 5 - 430675100);

        if (remainder > 0) {
            uint num = 0;
            switch (remainder) {
                case 1:
                    num ^= bytes[position];
                    break;
                case 2:
                    num ^= (uint)bytes[position + 1] << 8;
                    goto case 1;
                case 3:
                    num ^= (uint)bytes[position + 2] << 16;
                    goto case 2;
            }

            h1 ^= RotateLeft(num * 3432918353U, 15) * 461845907U;
        }

        h1 = FMix(h1 ^ (uint)length);

        return h1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint RotateLeft(uint x, byte r) {
        return (x << r) | (x >> (32 - r));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint FMix(uint h) {
        h = (uint)(((int)h ^ (int)(h >> 16)) * -2048144789);
        h = (uint)(((int)h ^ (int)(h >> 13)) * -1028477387);
        return h ^ (h >> 16);
    }
}