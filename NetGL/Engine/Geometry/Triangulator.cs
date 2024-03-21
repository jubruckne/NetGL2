using System.Numerics;
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

    private readonly List<TPosition> partial_quads = new(4);
    private readonly Dictionary<TPosition, TIndex> point_to_index_cache = new(ushort.MaxValue * 2);

    public bool eof => position_writer.eof || index_writer.eof;

    public Triangulator(ArrayView<TPosition> positions, ArrayView<Index<TIndex>> indices) {
        this.position_writer   = positions.new_writer();
        this.index_writer     = indices.new_writer();
        this.draw_ranges = new List<VertexArrayIndexed.DrawRange>();
        this.start_index = 0;
        this.base_vertex = 0;
    }

    public void partial_quad(in TPosition p) {
        partial_quads.Add(p);

        if (partial_quads.Count == 4) {
            quad(partial_quads[0], partial_quads[1], partial_quads[2], partial_quads[3]);
            partial_quads.Clear();
        }
    }

    public void quad(in TPosition p0, in TPosition p1, in TPosition p2, in TPosition p3) {
        if (position_writer.position - base_vertex + 3 >= Index<TIndex>.max_vertex_count) {
            draw_ranges.writeable().Add(new VertexArrayIndexed.DrawRange(start_index, index_writer.position - 1, base_vertex));
            start_index = index_writer.position;
            base_vertex =  position_writer.position;
            point_to_index_cache.Clear();
        }

        var (i0, i1, i2, i3) = lookup_or_allocate(p0, p1, p2, p3, base_vertex);

        //Console.WriteLine($"iw {index_writer}, i0:{i0}");

        index_writer.write(
                      (i0, i1, i2),
                      (i2, i3, i0)
                     );
    }

    private (TIndex i0, TIndex i1, TIndex i2, TIndex i3) lookup_or_allocate(in TPosition p0,
                                                                            in TPosition p1,
                                                                            in TPosition p2,
                                                                            in TPosition p3,
                                                                            int base_vertex
    ) {
        int i;

        if (!point_to_index_cache.TryGetValue(p0, out var i0)) {
            i = position_writer.write(p0) - base_vertex;
            i0 = TIndex.CreateChecked(i);
            point_to_index_cache.Add(p0, i0);
        }

        if (!point_to_index_cache.TryGetValue(p1, out var i1)) {
            i  = position_writer.write(p1) - base_vertex;
            i1 = TIndex.CreateChecked(i);
            point_to_index_cache.Add(p1, i1);
        }

        if (!point_to_index_cache.TryGetValue(p2, out var i2)) {
            i  = position_writer.write(p2) - base_vertex;
            i2 = TIndex.CreateChecked(i);
            point_to_index_cache.Add(p2, i2);
        }

        if (!point_to_index_cache.TryGetValue(p3, out var i3)) {
            i  = position_writer.write(p3) - base_vertex;
            i3 = TIndex.CreateChecked(i);
            point_to_index_cache.Add(p3, i3);
        }

        return (i0, i1, i2, i3);
    }

    public List<VertexArrayIndexed.DrawRange> finish() {
        draw_ranges.Add(new(start_index, index_writer.position - 1, base_vertex));

        Error.assert(partial_quads, partial_quads.Count == 0);
        Error.assert(index_writer, index_writer.eof);

        // difficult to calculate exactly
        // Error.assert(position_writer, position_writer.eof);

        index_writer.rewind();
        position_writer.rewind();
        start_index = 0;
        base_vertex = 0;

        var list = new List<VertexArrayIndexed.DrawRange>(draw_ranges);
        draw_ranges.Clear();
        point_to_index_cache.Clear();
        return list;
    }
}