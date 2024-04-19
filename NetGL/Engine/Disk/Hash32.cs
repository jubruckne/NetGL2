using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NetGL;

public readonly struct HashCode32 {
    private readonly uint hash;

    // 8 bit for type hash, 24 bit for value hash
    private byte type_hash => (byte)(hash >> 24);
    private uint value_hash => hash & 0x00FFFFFF;

    private HashCode32(uint type, uint value)
        => hash = ((type << 24) ^ (type << 16) ^ (type << 8) ^ type) | value;

    public static HashCode32 of<T>(T data) where T: unmanaged
        => new(Murmur.hash32(typeof(T).FullName), Murmur.hash32(data));

    public static HashCode32 of(string str)
        => new(Murmur.hash32("string"), Murmur.hash32(str));

    public static HashCode32 of<T>(ReadOnlySpan<T> data)
        where T: unmanaged
        => new(Murmur.hash32($"{typeof(T).FullName}[]"), Murmur.hash32(MemoryMarshal.AsBytes(data)));

    public bool is_type<T>() where T: unmanaged
        => type_hash == HashCode32.of(new T()).type_hash;

    public bool verify<T>(T data) where T: unmanaged
        => type_hash == Murmur.hash32(typeof(T).FullName) && value_hash == Murmur.hash32(data);

    public bool verify(string str)
        => type_hash == Murmur.hash32("string") && value_hash == Murmur.hash32(str);

    public bool verify<T>(ReadOnlySpan<T> data)
        where T: unmanaged
        => type_hash == Murmur.hash32($"{typeof(T).FullName}[]")
           && value_hash == Murmur.hash32(MemoryMarshal.AsBytes(data));

    public static bool operator ==(HashCode32 left, HashCode32 right) => left.hash == right.hash;
    public static bool operator !=(HashCode32 left, HashCode32 right) => left.hash != right.hash;

    public override string ToString()
        => $"{type_hash:X4}-{value_hash:X4}";
}

public readonly struct HashCode64 {
    private readonly ulong hash;

    private uint type_hash => (uint)(hash >> 32);
    private uint value_hash => (uint)hash;

    private HashCode64(uint type, uint value)
        => hash = ((ulong)type << 32) | value;

    public static HashCode64 of<T>(T data) where T: unmanaged
        => new(Murmur.hash32(typeof(T).FullName), Murmur.hash32(data));

    public static HashCode64 of(string str)
        => new(Murmur.hash32("string"), Murmur.hash32(str));

    public static HashCode64 of<T>(ReadOnlySpan<T> data)
        where T: unmanaged
        => new(Murmur.hash32($"{typeof(T).FullName}[]"), Murmur.hash32(MemoryMarshal.AsBytes(data)));

    public bool is_type<T>() where T: unmanaged
        => type_hash == HashCode64.of(new T()).type_hash;

    public bool verify<T>(T data) where T: unmanaged
        => type_hash == Murmur.hash32(typeof(T).FullName) && value_hash == Murmur.hash32(data);

    public bool verify(string str)
        => type_hash == Murmur.hash32("string") && value_hash == Murmur.hash32(str);

    public bool verify<T>(ReadOnlySpan<T> data)
        where T: unmanaged
        => type_hash == Murmur.hash32($"{typeof(T).FullName}[]")
           && value_hash == Murmur.hash32(MemoryMarshal.AsBytes(data));

    public static bool operator ==(HashCode64 left, HashCode64 right) => left.hash == right.hash;
    public static bool operator !=(HashCode64 left, HashCode64 right) => left.hash != right.hash;

    public override string ToString()
        => $"{type_hash:X8}-{value_hash:X8}";
}

public static class Murmur {
    private const uint seed = 4203733937;
    public static uint hash32<T>(T data)
        where T: unmanaged {
        var span = MemoryMarshal.CreateReadOnlySpan(ref data, 1);
        return hash32(MemoryMarshal.AsBytes(span));
    }

    public static uint hash32(ReadOnlySpan<char> bytes)
        => hash32(MemoryMarshal.AsBytes(bytes));

    public static uint hash32(ReadOnlySpan<byte> bytes) {
        var length    = bytes.Length;
        var h1        = seed;
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