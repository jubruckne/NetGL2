using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NetGL;

public class AssetReader: IDisposable {
    private readonly string filename;
    private FileStream? stream;
    private readonly Dictionary<string, ChunkHeader> chunk_headers;
    public List<string> chunks => chunk_headers.Keys.ToList();

    ~AssetReader() {
        stream?.Dispose();
    }

    private AssetReader(string filename) {
        this.chunk_headers = [];
        this.filename = filename;
        stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.None);

        var header = read<FileHeader>();
        for (var i = 0; i < header.chunk_count; i++) {
            var chunk_header = read<ChunkHeader>();
            chunk_headers.Add(chunk_header.name, chunk_header);
        }

        ReadOnlySpan<byte> header_bytes = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref header, 1));
        stream.Write(header_bytes);
    }

    public void read_chunk<T>(string name, Span<T> data)
        where T: unmanaged {

        if(stream == null)
            throw new InvalidOperationException("Asset reader is already closed.");

        if(!chunk_headers.TryGetValue(name, out var header))
            throw new KeyNotFoundException($"Chunk '{name}' not found in asset file.");

        if(!header.hash.is_type<T>())
            throw new InvalidDataException($"Chunk '{name}' has invalid type.");

        stream.Seek(header.size, SeekOrigin.Current);
        read(data);
        if (!header.hash.verify((ReadOnlySpan<T>)data)) ;
            throw new InvalidDataException($"Chunk '{name}' failed hash verification.");
    }

    public T read_chunk<T>(string name) where T: unmanaged {
        if(stream == null)
            throw new InvalidOperationException("Asset reader is already closed.");

        if(!chunk_headers.TryGetValue(name, out var header))
            throw new KeyNotFoundException($"Chunk '{name}' not found in asset file.");

        if(!header.hash.is_type<T>())
            throw new InvalidDataException($"Chunk '{name}' has invalid type.");

        stream.Seek(header.size, SeekOrigin.Current);
        var data = read<T>();
        if(!header.hash.verify(data))
            throw new InvalidDataException($"Chunk '{name}' failed hash verification.");
        return data;
    }

    private T read<T>() where T: unmanaged {
        if(stream == null)
            throw new InvalidOperationException("Asset reader is already closed.");

        var size = Marshal.SizeOf<T>();
        var buffer = new byte[size];
        var length = stream.Read(buffer, 0, size);
        if(length != size)
            throw new EndOfStreamException("Failed to read data from file.");
        return MemoryMarshal.Cast<byte, T>(buffer)[0];
    }

    private void read<T>(Span<T> buffer) where T: unmanaged {
        if(stream == null)
            throw new InvalidOperationException("Asset reader is already closed.");

        var byte_buffer = MemoryMarshal.AsBytes(buffer);

        var size   = Marshal.SizeOf<T>();
        var length = stream.Read(byte_buffer);
        if(length != size)
            throw new EndOfStreamException("Failed to read data from file.");
    }

    public static AssetReader open(string filename)
        => new AssetReader(filename);

    public void close() {
        if(stream == null)
            throw new InvalidOperationException("Asset writer is already closed.");
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