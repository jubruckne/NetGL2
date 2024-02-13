using OpenTK.Mathematics;

namespace NetGL;

public interface IShape <T> {
    IShapeGenerator generate();
}

public interface IShapeGenerator {
    IEnumerable<Vector3> get_vertices();
    IEnumerable<Vector3i> get_indices();

    IEnumerable<Vector3> get_faces() {
        var vert = get_vertices().ToList();

        foreach (var idx in get_indices()) {
            yield return new Vector3(vert[idx.X]);
            yield return new Vector3(vert[idx.Y]);
            yield return new Vector3(vert[idx.Z]);
        }
    }

    IEnumerable<Struct<Vector3, Vector3>> get_vertices_and_normals() {
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
            yield return new Struct<Vector3, Vector3>(vertices[i], Vector3.Normalize(normals[i]));
        }
    }
}