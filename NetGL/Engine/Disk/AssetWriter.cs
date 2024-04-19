using System.Runtime.InteropServices;

namespace NetGL;

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
        hash = Murmur.hash32(filename);

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

        var type = Murmur.hash32(typeof(T).get_type_name());

        var data_bytes = MemoryMarshal.AsBytes(data);

        write(name, type, data_bytes);
    }

    private void write(string name, uint type, ReadOnlySpan<byte> data) {
        if(stream == null)
            throw new InvalidOperationException("Asset writer is already closed.");

        if(name.Length > 64)
            throw new ArgumentException("Chunk name is too long.", nameof(name));

        ChunkHeader header = new (name, (uint)data.Length, HashCode64.of(data));
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
}