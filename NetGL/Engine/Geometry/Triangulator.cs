using System.Numerics;
using System.Runtime.CompilerServices;
using NetGL.Vectors;

namespace NetGL;

public static class Triangulator {
    public static float3 calculate_normal(in float3 p0, in float3 p1, in float3 p2) {
        var edge1 = p1 - p0;
        var edge2 = p2 - p0;
        var norm  = cross(edge1, edge2);
        norm.normalize();
        return norm;
    }

    public static void calculate_normals(this VertexBuffer<float3, half3> vb, in ArrayView<Index<ushort>> indices) {
        var positions = vb.positions;
        var normals   = vb.normals;

        foreach (var tri in indices) {
            var edge1  = positions[tri.p1] - positions[tri.p0];
            var edge2  = positions[tri.p2] - positions[tri.p0];
            var normal = cross(edge1,edge2);

            normals[tri.p0] += (half3)normal;
            normals[tri.p1] += (half3)normal;
            normals[tri.p2] += (half3)normal;
        }

        for (var i = 0; i < normals.length; i++) {
            normals[i].normalize();
        }
    }

    public static void calculate_normals(this VertexBuffer<float3, float3> vb, in ArrayView<Index<ushort>> indices) {
        var positions = vb.positions;
        var normals = vb.normals;

        foreach (var tri in indices) {
            var edge1  = positions[tri.p1] - positions[tri.p0];
            var edge2  = positions[tri.p2] - positions[tri.p0];
            var normal = cross(edge1,edge2);

            normals[tri.p0] += normal;
            normals[tri.p1] += normal;
            normals[tri.p2] += normal;
        }

        for (var i = 0; i < normals.length; i++) {
            normals[i].normalize();
        }
    }

    public static void calculate_normals(this VertexBuffer<float3, float3> vb, in ArrayView<Index<int>> indices) {
        var positions = vb.positions;
        var normals   = vb.normals;

        foreach (var tri in indices) {
            var edge1  = positions[tri.p1] - positions[tri.p0];
            var edge2  = positions[tri.p2] - positions[tri.p0];
            var normal = cross(edge1,edge2);

            normals[tri.p0] += normal;
            normals[tri.p1] += normal;
            normals[tri.p2] += normal;
        }

        for (var i = 0; i < normals.length; i++) {
            normals[i].normalize();
        }
    }
}

public sealed class Triangulator<TPosition, TIndex>
    where TPosition: unmanaged
    where TIndex: unmanaged, IBinaryInteger<TIndex> {

    private readonly ArrayWriter<TPosition> position_writer;
    private readonly ArrayWriter<Index<TIndex>> index_writer;
    private readonly VertexArrayIndexed.DrawRanges draw_ranges;

    private int base_vertex;
    private int start_index;

    public Triangulator(ArrayView<TPosition> positions, ArrayView<Index<TIndex>> indices) {
        position_writer = positions.new_writer();
        index_writer    = indices.new_writer();
        draw_ranges     = new VertexArrayIndexed.DrawRanges();
        start_index     = 0;
        base_vertex     = 0;
    }

    public int vertex(in TPosition p) {
        if (position_writer.position - base_vertex + 3 >= Index<TIndex>.max_vertex_count) {
            draw_ranges.add(start_index, index_writer.position - 1, base_vertex);
            start_index = index_writer.position;
            base_vertex = position_writer.position;
        }

        return position_writer.write(p);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int triangle(in Index<int> i) {
        var index = new Index<TIndex>(
                                      TIndex.CreateChecked(i.p0 % Index<TIndex>.max_vertex_count),
                                      TIndex.CreateChecked(i.p1 % Index<TIndex>.max_vertex_count),
                                      TIndex.CreateChecked(i.p2 % Index<TIndex>.max_vertex_count)
                                     );
       return index_writer.write(index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int triangle(int p0, int p1, int p2) {
        var index = new Index<TIndex>(
                                      TIndex.CreateChecked(p0 % Index<TIndex>.max_vertex_count),
                                      TIndex.CreateChecked(p1 % Index<TIndex>.max_vertex_count),
                                      TIndex.CreateChecked(p2 % Index<TIndex>.max_vertex_count)
                                     );
        return index_writer.write(index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void quad(int p0, int p1, int p2, int p3) {
        triangle(p2, p1, p0);
        triangle(p0, p3, p2);
    }

    public VertexArrayIndexed.DrawRanges finish() {
        if (index_writer.position != start_index) {
            // append final draw range
            draw_ranges.add(start_index, index_writer.position - 1, base_vertex);
        }

        Debug.assert(index_writer, index_writer.eof);
        Debug.assert(position_writer, position_writer.eof);

        index_writer.rewind();
        position_writer.rewind();
        start_index = 0;
        base_vertex = 0;

        var list = new VertexArrayIndexed.DrawRanges(draw_ranges);
        draw_ranges.clear();
        return list;
    }
}