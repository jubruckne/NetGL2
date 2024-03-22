using System.Numerics;
using System.Runtime.CompilerServices;
using Vector3 = OpenTK.Mathematics.Vector3;

namespace NetGL;

public static class Triangulator {
    public static void calculate_normals(this VertexBuffer<Vector3, Vector3> vb, in ArrayView<Index<ushort>> indices) {
        var positions = vb.positions;
        var normals = vb.normals;

        foreach (var tri in indices) {
            var edge1  = positions[tri.p2] - positions[tri.p1];
            var edge2  = positions[tri.p3] - positions[tri.p1];
            var normal = Vector3.Cross(edge1,edge2);

            normals[tri.p1] += normal;
            normals[tri.p2] += normal;
            normals[tri.p3] += normal;
        }

        for (var i = 0; i < normals.length; i++) {
            normals[i].Normalize();
        }
    }

    public static void calculate_normals(this VertexBuffer<Vector3, Vector3> vb, in ArrayView<Index<int>> indices) {
        var positions = vb.positions;
        var normals   = vb.normals;

        foreach (var tri in indices) {
            var edge1  = positions[tri.p2] - positions[tri.p1];
            var edge2  = positions[tri.p3] - positions[tri.p1];
            var normal = Vector3.Cross(edge1,edge2);

            normals[tri.p1] += normal;
            normals[tri.p2] += normal;
            normals[tri.p3] += normal;
        }

        for (var i = 0; i < normals.length; i++) {
            normals[i].Normalize();
        }
    }
}

public sealed class Triangulator<TPosition, TIndex>
    where TPosition: unmanaged
    where TIndex: unmanaged, IBinaryInteger<TIndex> {

    private readonly ArrayWriter<TPosition> position_writer;
    private readonly ArrayWriter<Index<TIndex>> index_writer;
    private readonly List<VertexArrayIndexed.DrawRange> draw_ranges;

    private int base_vertex;
    private int start_index;

    public Triangulator(ArrayView<TPosition> positions, ArrayView<Index<TIndex>> indices) {
        position_writer = positions.new_writer();
        index_writer    = indices.new_writer();
        draw_ranges     = new List<VertexArrayIndexed.DrawRange>();
        start_index     = 0;
        base_vertex     = 0;
    }

    public int vertex(in TPosition p) {
        if (position_writer.position - base_vertex + 3 >= Index<TIndex>.max_vertex_count) {
            draw_ranges.Add(new VertexArrayIndexed.DrawRange(start_index, index_writer.position - 1, base_vertex));
            start_index = index_writer.position;
            base_vertex = position_writer.position;
        }

        return position_writer.write(p);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int triangle(in Index<TIndex> i) => index_writer.write(i);

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void triangle(TIndex p0, TIndex p1, TIndex p2) => index_writer.write(new Index<TIndex>(p0, p1, p2));

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void quad(in Index<TIndex> i0, in Index<TIndex> i1) {
        index_writer.write(i0);
        index_writer.write(i1);
    }

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void quad(TIndex p0, TIndex p1, TIndex p2, TIndex p3) {
        index_writer.write(new Index<TIndex>(p0, p1, p2));
        index_writer.write(new Index<TIndex>(p2, p3, p0));
    }

    public List<VertexArrayIndexed.DrawRange> finish() {
        draw_ranges.Add(new(start_index, index_writer.position - 1, base_vertex));

        Error.assert(index_writer, index_writer.eof);
        Error.assert(position_writer, position_writer.eof);

        index_writer.rewind();
        position_writer.rewind();
        start_index = 0;
        base_vertex = 0;

        var list = new List<VertexArrayIndexed.DrawRange>(draw_ranges);
        draw_ranges.Clear();
        return list;
    }
}