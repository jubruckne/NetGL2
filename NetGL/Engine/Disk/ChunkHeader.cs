using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace NetGL;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 64)]
internal unsafe struct ChunkHeader {
    private fixed byte _name[48];

    public string name {
        get {
            fixed(void* p = _name) {
                return System.Text.Encoding.ASCII.GetString((byte*)p, 48);
            }
        }
    }

    public readonly uint size;
    public readonly HashCode64 hash;

    [SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable")]
    private readonly uint __padding = uint.MaxValue;

    public ChunkHeader(string name, uint size, HashCode64 hash) {
        Debug.assert_equal(sizeof(ChunkHeader), 64);

        this.hash = hash;
        var ascii_bytes = System.Text.Encoding.ASCII.GetBytes(name);

        fixed(void* p = _name) {
            var span = new Span<byte>(p, 48);
            span.Fill((byte)' ');
            ascii_bytes.CopyTo(span);
        }
        this.size = size;
    }
}