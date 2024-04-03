using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.Arm;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

[MemoryDiagnoser(true)]
public class Md5VsSha256 {
    private const int N = 1000;
    private readonly byte[] data;

    public Md5VsSha256() {
        data = new byte[N];
        new Random(42).NextBytes(data);
    }

    [Benchmark]
    public byte[] Crc32() => System.IO.Hashing.Crc32.Hash(data);

    [Benchmark]
    public byte[] XxHash32() => System.IO.Hashing.XxHash32.Hash(data);

    [Benchmark]
    public int HashCode() => System.HashCode.Combine(data);

    [Benchmark]
    public uint SuperFastHashSimd_1() => SuperFastHashSimd.ComputeHash(data);
    [Benchmark]
    public uint SuperFastHashSimd_2() => SuperFastHashSimd2.ComputeHash(data);

}

public class Program
{
    public static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<Md5VsSha256>();
    }
}

public static class SuperFastHashSimd
{
    public static uint ComputeHash(byte[] data)
    {
        int length = data.Length;
        if (length == 0)
            return 0;

        uint hash          = (uint)length;
        int  remainder     = length & 3;
        int  alignedLength = length - remainder;
        int  vectorSize    = Vector<byte>.Count;
        int  i             = 0;

        // Process in chunks using SIMD
        for (; i <= alignedLength - vectorSize; i += vectorSize)
        {
            Vector<byte> chunk = new Vector<byte>(data, i);
            Vector<uint> k     = Vector.AsVectorUInt32(chunk);
            k *= 0xcc9e2d51;
            // Simulate the RotateLeft operation
            Vector<uint> leftShift  = Vector.BitwiseAnd(k << 15, new Vector<uint>(uint.MaxValue));
            Vector<uint> rightShift = Vector.BitwiseAnd(k >> 17, new Vector<uint>(uint.MaxValue));
            k = leftShift | rightShift;
            k *= 0x1b873593;

            // Reduce the vector k into a single uint hash
            for (int j = 0; j < Vector<uint>.Count; j++)
            {
                hash ^= k[j];
                hash =  (hash << 13) | (hash >> 19);
                hash =  hash * 5 + 0xe6546b64;
            }
        }

        // Process the remaining bytes (if any)
        if (remainder > 0)
        {
            uint k = 0;
            switch (remainder)
            {
                case 3: k ^= (uint)data[i + 2] << 16; goto case 2;
                case 2: k ^= (uint)data[i + 1] << 8; goto case 1;
                case 1: k ^= (uint)data[i];
                    k     *= 0xcc9e2d51;
                    k     =  (k << 15) | (k >> 17);
                    k     *= 0x1b873593;
                    hash  ^= k;
                    break;
            }
        }

        // Finalization mix
        hash ^= (uint)length;
        hash ^= hash >> 16;
        hash *= 0x85ebca6b;
        hash ^= hash >> 13;
        hash *= 0xc2b2ae35;
        hash ^= hash >> 16;

        return hash;
    }
}


public static class SuperFastHashSimd2
{
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static uint ComputeHash<T>(in T[] data_)
        where T: struct {
        ReadOnlySpan<byte> data = MemoryMarshal.AsBytes<T>(data_);
        int length = data.Length;
        if (length == 0)
            return 0;

        uint hash          = (uint)length;
        int  remainder     = length & 3;
        int  alignedLength = length - remainder;
        int  vectorSize    = Vector<byte>.Count;
        int  i             = 0;
        int count = Vector<uint>.Count;

        // Process in chunks using SIMD
        Vector<uint> k;
        for (; i <= alignedLength - vectorSize; i += vectorSize) {
            k = new Vector<uint>(data.Slice(i, vectorSize)) * 0xcc9e2d51;
            k = ((k << 15) | (k >> 17)) * 0x1b873593;

            // Reduce the vector k into a single uint hash
            for (var j = 0; j < count; j++) {
                hash ^= k[j];
                hash = ((hash << 13) | (hash >> 19)) * 5 + 0xe6546b64;
            }
        }

        // Process the remaining bytes (if any)
        if (remainder > 0) {
            uint k2 = 0;
            switch (remainder) {
                case 3: k2 ^= (uint)data[i + 2] << 16; goto case 2;
                case 2: k2 ^= (uint)data[i + 1] << 8; goto case 1;
                case 1: k2 ^= data[i];
                    k2     *= 0xcc9e2d51;
                    k2     =  (k2 << 15) | (k2 >> 17);
                    k2     *= 0x1b873593;
                    hash  ^= k2;
                    break;
            }
        }

        // Finalization mix
        hash ^= (uint)length;
        hash ^= hash >> 16;
        hash *= 0x85ebca6b;
        hash ^= hash >> 13;
        hash *= 0xc2b2ae35;
        hash ^= hash >> 16;

        return hash;
    }
}