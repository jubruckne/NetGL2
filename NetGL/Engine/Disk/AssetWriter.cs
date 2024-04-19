using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using BulletSharp;

namespace NetGL;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 32)]
internal unsafe struct FileHeader {
    public readonly uint signature;
    public readonly uint chunk_count;
    public readonly uint hash;

    public fixed byte padding[20];

    public FileHeader(ushort version) {
        Debug.assert_equal(sizeof(FileHeader), 32);

        const int magic = 'J' | ('L' << 8);
        this.signature = (uint)((magic << 16) | version);
        chunk_count = 0;
        hash = 0;
        fixed(void* p = padding) {
            Span<byte> padding_bytes = new(p, 20);
            padding_bytes.Fill((byte)' ');

        }
    }

    public FileHeader(ushort version, uint chunk_count, uint hash): this(version) {
        this.chunk_count = chunk_count;
        this.hash = hash;
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 64)]
internal unsafe struct ChunkHeader {
    public fixed byte name[48];
    public readonly uint type;
    public readonly uint size;
    public readonly uint hash;
    [SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable")]
    private readonly uint padding = uint.MaxValue;

    public ChunkHeader(string name, uint type, uint size, uint hash) {
        Debug.assert_equal(sizeof(ChunkHeader), 64);

        this.type = type;
        this.hash = hash;
        var utf8Bytes = System.Text.Encoding.UTF8.GetBytes(name);

        fixed(void* p = this.name) {
            var span = new Span<byte>(p, 48);
            span.Fill((byte)' ');
            utf8Bytes.CopyTo(span);
        }
        this.size = size;
    }
}

public class AssetWriter: IDisposable {
    private readonly string filename;
    private FileStream? stream;
    private readonly List<ChunkHeader> chunk_headers;
    private readonly uint hash;

    ~AssetWriter() {
        stream?.Dispose();
    }

    private AssetWriter(string filename) {
        this.filename = filename;
        chunk_headers = [];
        stream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None);

        var header = new FileHeader();
        hash = hash32(filename);

        ReadOnlySpan<byte> header_bytes = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref header, 1));
        stream.Write(header_bytes);
    }

    public void write(string name, string data)
        => write(name, System.Text.Encoding.UTF8.GetBytes(data).AsSpan());

    public void write<T>(string name, T data) where T: unmanaged
        => write(name, new ReadOnlySpan<T>(ref data));

    public void write<T>(string name, Span<T> data) where T: unmanaged {
        ReadOnlySpan<T> span = data;
        write(name, span);
    }

    public void write<T>(string name, ReadOnlySpan<T> data) where T: unmanaged {
        if(this.stream == null)
            throw new InvalidOperationException("Asset writer is already closed.");

        var type = hash32(typeof(T).get_type_name());

        var data_bytes = MemoryMarshal.AsBytes(data);

        write(name, type, data_bytes);
    }

    private void write(string name, uint type, ReadOnlySpan<byte> data) {
        if(stream == null)
            throw new InvalidOperationException("Asset writer is already closed.");

        if(name.Length > 64)
            throw new ArgumentException("Chunk name is too long.", nameof(name));

        ChunkHeader header = new (name, type, (uint)data.Length, hash32(data));
        stream.Write(MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref header, 1)));
        stream.Write(data);
        chunk_headers.Add(header);
    }

    public static AssetWriter open(string filename)
        => new AssetWriter(filename);

    public void close() {
        if(stream == null)
            throw new InvalidOperationException("Asset writer is already closed.");

        stream.Seek(0, SeekOrigin.Begin);

        FileHeader file_header = new(1, (uint)chunk_headers.Count, hash);

        stream.Write(MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref file_header, 1)));
        stream.Close();
        stream = null;
    }

    void IDisposable.Dispose() {
        if(stream != null)
            close();
    }

    private static uint hash32(ReadOnlySpan<char> bytes, uint seed = 4203733937)
        => hash32(MemoryMarshal.AsBytes(bytes), seed);

    private static uint hash32(ReadOnlySpan<byte> bytes, uint seed = 4203733937) {
        var length    = bytes.Length;
        var h1        = seed;
        var remainder = length & 3;
        var position  = length - remainder;
        for (var start = 0; start < position; start += 4)
            h1 = (uint) ((int) RotateLeft(h1 ^ RotateLeft(BitConverter.ToUInt32(bytes.Slice(start, 4)) * 3432918353U,15) * 461845907U, 13) * 5 - 430675100);

        if (remainder > 0) {
            uint num = 0;
            switch (remainder) {
                case 1:
                    num ^= (uint) bytes[position];
                    break;
                case 2:
                    num ^= (uint) bytes[position + 1] << 8;
                    goto case 1;
                case 3:
                    num ^= (uint) bytes[position + 2] << 16;
                    goto case 2;
            }

            h1 ^= RotateLeft(num * 3432918353U, 15) * 461845907U;
        }

        h1 = FMix(h1 ^ (uint) length);

        return h1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint RotateLeft(uint x, byte r) {
        return x << (int) r | x >> 32 - (int) r;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint FMix(uint h) {
        h = (uint) (((int) h ^ (int) (h >> 16)) * -2048144789);
        h = (uint) (((int) h ^ (int) (h >> 13)) * -1028477387);
        return h ^ h >> 16;
    }
}