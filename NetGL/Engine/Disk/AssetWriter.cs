using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NetGL;

public class AssetWriter: IDisposable {
    private readonly string filename;
    private FileStream? stream;
    private readonly List<ChunkHeader> chunk_headers;

    ~AssetWriter() {
        stream?.Dispose();
    }

    private AssetWriter(string filename) {
        this.filename = filename;
        chunk_headers = [];
        stream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None);

        var header = new FileHeader();

        ReadOnlySpan<byte> header_bytes = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref header, 1));
        stream.Write(header_bytes);
    }

    public void write_chunk(string name, string data)
        => write_chunk(name, HashCode64.of(data), System.Text.Encoding.UTF8.GetBytes(data).AsSpan());

    public void write_chunk<T>(string name, T data)
        where T: unmanaged {
        var span = new ReadOnlySpan<T>(ref data);
        write_chunk(name, HashCode64.of(data), MemoryMarshal.AsBytes(span));
    }

    public void write_chunk<T>(string name, Span<T> data) where T: unmanaged {
        ReadOnlySpan<T> span = data;
        write_chunk(name, HashCode64.of(span), MemoryMarshal.AsBytes(span));
    }

    public void write_chunk<T>(string name, ReadOnlySpan<T> data) where T: unmanaged {
        if(stream == null)
            throw new InvalidOperationException("Asset writer is already closed.");

        write_chunk(name, HashCode64.of(data), MemoryMarshal.AsBytes(data));
    }

    private void write_chunk(string name, HashCode64 hash, ReadOnlySpan<byte> data) {
        if(stream == null)
            throw new InvalidOperationException("Asset writer is already closed.");

        if(name.Length > 64)
            throw new ArgumentException("Chunk name is too long.", nameof(name));

        ChunkHeader header = new (name, (uint)data.Length, (uint)(stream.Position + Unsafe.SizeOf<ChunkHeader>()), hash);
        stream.Write(MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref header, 1)));
        stream.Write(data);
        chunk_headers.Add(header);

        Console.WriteLine($"Wrote chunk '{name}' with {data.Length} bytes, hash: {hash}.");
    }

    public static AssetWriter open(string filename)
        => new AssetWriter(filename);

    public void close() {
        if(stream == null)
            throw new InvalidOperationException("Asset writer is already closed.");

        stream.Seek(0, SeekOrigin.Begin);

        FileHeader file_header = new(1, (uint)chunk_headers.Count, HashCode32.of(filename));

        stream.Write(MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref file_header, 1)));
        stream.Close();
        stream = null;
    }

    void IDisposable.Dispose() {
        if(stream != null)
            close();
    }
}