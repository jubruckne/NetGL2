using System.Numerics;
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

    (VertexBuffer.Position_Normal<Vector3, Vector3> vb, IndexBuffer<int> ib) create();

    void fill<TPosition, TNormal>(
        in VertexBuffer.Position_Normal<TPosition, TNormal> vertex_buffer,
        in IndexBuffer<int> index_buffer)
        where TPosition : unmanaged
        where TNormal : unmanaged {

    }

}

public interface IShapeGenerator {
    IEnumerable<Vector3> get_vertices();
    IEnumerable<Vector3i> get_indices();

    List<Vector3> get_faces() {
        var list = new List<Vector3>();

        var vert = get_vertices().ToList();

        foreach (var idx in get_indices()) {
            list.Add(new Vector3(vert[idx.X]));
            list.Add(new Vector3(vert[idx.Y]));
            list.Add(new Vector3(vert[idx.Z]));
        }

        return list;
    }

    List<Struct<Vector3, Vector3>> get_vertices_and_normals() {
        var list = new List<Struct<Vector3, Vector3>>();

        var vertices = get_vertices().ToArray();
        var indices = get_indices().ToArray();
        var normals = new Vector3[vertices.Length];

        Vector3 edge2;
        Vector3 edge1;
        Vector3 faceNormal;

        foreach (var tri in indices) {
            var v1 = vertices[tri.X];
            var v2 = vertices[tri.Y];
            var v3 = vertices[tri.Z];

            edge1 = v2 - v1;
            edge2 = v3 - v1;
            faceNormal = Vector3.Cross(edge1, edge2);

            normals[tri.X] += faceNormal;
            normals[tri.Y] += faceNormal;
            normals[tri.Z] += faceNormal;
        }

        for (var i = 0; i < normals.Length; i++) {
            list.Add(new Struct<Vector3, Vector3>(vertices[i], Vector3.Normalize(normals[i])));
        }

        return list;
    }
}