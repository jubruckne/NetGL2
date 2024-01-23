using OpenTK.Mathematics;

namespace NetGL;

public class Cube: IShape<Cube> {
    public float width;
    public float height;
    public float depth;

    public Cube(float radius = 0.5f): this(radius * 2f, radius * 2f, radius * 2f) { }

    public Cube(float width, float height, float depth) {
        this.width = width;
        this.height = height;
        this.depth = depth;
    }

    public static (Vector3[] vertices, IndexBuffer.Triangle[] triangles) create_geometry(float radius = 0.5f, int segments = 1) {
        return create_geometry(new Cube(radius), segments);
    }

    public static (Vector3[] vertices, IndexBuffer.Triangle[] triangles) create_geometry(Cube cube, int segments = 1) {
        var halfWidth = cube.width * 0.5f;
        var halfHeight = cube.height * 0.5f;
        var halfDepth = cube.depth * 0.5f;

        // Define the eight vertices of the cube
        Vector3[] corners = [
            new Vector3(-halfWidth, -halfHeight, -halfDepth), // Bottom front left
            new Vector3(halfWidth, -halfHeight, -halfDepth),  // Bottom front right
            new Vector3(-halfWidth, halfHeight, -halfDepth),  // Top front left
            new Vector3(halfWidth, halfHeight, -halfDepth),   // Top front right
            new Vector3(-halfWidth, -halfHeight, halfDepth),  // Bottom back left
            new Vector3(halfWidth, -halfHeight, halfDepth),   // Bottom back right
            new Vector3(-halfWidth, halfHeight, halfDepth),   // Top back left
            new Vector3(halfWidth, halfHeight, halfDepth)     // Top back right
        ];

        // Define the indices for each face (two triangles per face) in clockwise order
        int[][] face_indices = [
            [0, 1, 2, 3], // Front face
            [4, 6, 5, 7], // Back face
            [1, 5, 3, 7], // Right face
            [0, 2, 4, 6], // Left face
            [0, 4, 1, 5], // Bottom face
            [2, 3, 6, 7]  // Top face
        ];

        IndexBuffer.Triangle[] indices = new IndexBuffer.Triangle[12]; // 12 triangles (2 per face) for 6 faces

        int index = 0;
        foreach (var face in face_indices) {
            indices[index++].set((ushort)face[0], (ushort)face[1], (ushort)face[2]); // First triangle
            indices[index++].set((ushort)face[2], (ushort)face[1], (ushort)face[3]); // Second triangle
        }

        Console.WriteLine($"cube\nvertices:\n");
        foreach(var v in corners)
            Console.WriteLine(v);
        Console.WriteLine("faces:\n");
        foreach(var i in indices)
            Console.WriteLine(indices);

        return (corners, indices);
    }
}