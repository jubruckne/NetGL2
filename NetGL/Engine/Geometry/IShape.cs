using System.Runtime.InteropServices;
using OpenTK.Mathematics;
using Vector3 = OpenTK.Mathematics.Vector3;

namespace NetGL;

public interface IShape {
    IShapeGenerator generate();
}

public interface IShape<in TGeneratorOptions> : IShape {
    IShapeGenerator generate(TGeneratorOptions options);
}

public interface IShapeGenerator2 {
    int get_vertex_count();
    int get_index_count();

    // (VertexBuffer.Position_Normal<Vector3, Vector3> vertex_buffer, IndexBuffer<TIndex> index_buffer) create<TIndex>() where TIndex: unmanaged, IBinaryInteger<TIndex>;
}

public interface IShapeGenerator {
    ReadOnlySpan<Vector3> get_vertices();
    ReadOnlySpan<Vector3i> get_indices();

    ReadOnlySpan<Vector3> get_faces() {
        var list = new List<Vector3>();

        var vert = get_vertices();

        foreach (var idx in get_indices()) {
            list.Add(new Vector3(vert[idx.X]));
            list.Add(new Vector3(vert[idx.Y]));
            list.Add(new Vector3(vert[idx.Z]));
        }

        return CollectionsMarshal.AsSpan(list);
    }

    ReadOnlySpan<Struct<Vector3, Vector3>> get_vertices_and_normals() {
        var vertices = get_vertices();
        var indices = get_indices();

        var list = new Struct<Vector3, Vector3>[vertices.Length];

        Vector3 edge2;
        Vector3 edge1;
        Vector3 faceNormal;

        Vector3 v1;
        Vector3 v2;
        Vector3 v3;

        foreach (var tri in indices) {
            v1 = vertices[tri.X];
            v2 = vertices[tri.Y];
            v3 = vertices[tri.Z];

            edge1 = v2 - v1;
            edge2 = v3 - v1;
            faceNormal = Vector3.Cross(edge1, edge2);

            list[tri.X].b += faceNormal;
            list[tri.Y].b += faceNormal;
            list[tri.Z].b += faceNormal;
        }

        for (var i = 0; i < vertices.Length; i++) {
            list[i].a = vertices[i];
            list[i].b.Normalize();
        }

        return list.AsSpan();
    }
}