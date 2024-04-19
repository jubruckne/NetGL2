using System.Runtime.InteropServices;

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
        signature = (uint)((magic << 16) | version);
        chunk_count    = 0;
        hash           = 0;
        fixed(void* p = padding) {
            Span<byte> padding_bytes = new(p, 20);
            padding_bytes.Fill((byte)' ');

        }
    }

    public FileHeader(ushort version, uint chunk_count, uint hash): this(version) {
        this.chunk_count = chunk_count;
        this.hash        = hash;
    }
}