using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using NetGL.Vectors;

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
            stream.Seek(chunk_header.size, SeekOrigin.Current);
        }
    }

    public void read_chunk<T>(string name, Span<T> buffer)
        where T: unmanaged {

        if(stream == null)
            throw new InvalidOperationException("Asset reader is already closed.");

        if(!chunk_headers.TryGetValue(name, out var header))
            throw new KeyNotFoundException($"Chunk '{name}' not found in asset file.");

        if (buffer.Length * Unsafe.SizeOf<T>() != header.size)
            throw new ArgumentException("Data buffer size does not match chunk size.", nameof(buffer));

        stream.Seek(header.offset, SeekOrigin.Begin);

        read(buffer);
        //Console.WriteLine(name + " hash: " + header.hash + " new hash: " + HashCode64.of<T>(buffer));

        var verification_hash = HashCode64.of<T>(buffer);
        if (header.hash != verification_hash)
            throw new InvalidDataException($"Chunk '{name}' failed hash verification.");
    }

    public unsafe string read_chunk(string name) {
        if(!chunk_headers.TryGetValue(name, out var header))
            throw new KeyNotFoundException($"Chunk '{name}' not found in asset file.");

        var buffer =
            header.size <= 16
            ? stackalloc byte[(int)header.size]
            : new byte[header.size];

        read_chunk(name, buffer);

        return System.Text.Encoding.UTF8.GetString(buffer);
    }

    public T read_chunk<T>(string name) where T: unmanaged {
        if(stream == null)
            throw new InvalidOperationException("Asset reader is already closed.");

        if(!chunk_headers.TryGetValue(name, out var header))
            throw new KeyNotFoundException($"Chunk '{name}' not found in asset file.");

        //Console.WriteLine(name + " hash: " + header.hash);

        stream.Seek(header.offset, SeekOrigin.Begin);
        var data = read<T>();
        if(header.hash != HashCode64.of(data))
            throw new InvalidDataException($"Chunk '{name}' failed hash verification.");
        return data;
    }

    private T read<T>() where T: unmanaged {
        if(stream == null)
            throw new InvalidOperationException("Asset reader is already closed.");

        var size = Marshal.SizeOf<T>();
        var buffer = size <= 32 ? stackalloc byte[size] : new byte[size];

        if (buffer == null) throw new ArgumentNullException(nameof(buffer));
        var length = stream.Read(buffer);
        if(length != size)
            throw new EndOfStreamException("Failed to read data from file.");
        return MemoryMarshal.Cast<byte, T>(buffer)[0];
    }

    private void read<T>(Span<T> buffer) where T: unmanaged {
        if(stream == null)
            throw new InvalidOperationException("Asset reader is already closed.");

        var byte_buffer = MemoryMarshal.AsBytes(buffer);

        var size   = Marshal.SizeOf<T>() * buffer.Length;
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
}