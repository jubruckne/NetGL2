using OpenTK.Mathematics;

namespace NetGL;

public class Sphere: IShape<Sphere> {
    public readonly float radius;

    public Sphere(float radius) {
        this.radius = radius;
    }

    public static (Vector3[] vertices, IndexBuffer.Triangle[] triangles) create_geometry(Sphere sphere, int meridians, int parallels) {
        return create_geometry(sphere.radius, meridians, parallels);
    }

    public static (Vector3[] vertices, IndexBuffer.Triangle[] triangles) create_geometry(float radius, int meridians, int parallels) {
        int vertexCount = (meridians + 1) * (parallels + 1);

        var vertices = new Vector3[vertexCount];

        float x, y, z, xy;
        float u, v;
        float sectorStep = (float) (2f * Math.PI / meridians);
        float stackStep = (float) (Math.PI / parallels);

        for (int i = 0; i <= parallels; ++i) {
            float stackAngle = (float) (Math.PI / 2f - i * stackStep);
            xy = radius * (float) Math.Cos(stackAngle);
            z = radius * (float) Math.Sin(stackAngle);

            for (int j = 0; j <= meridians; ++j) {
                float sectorAngle = j * sectorStep;

                x = xy * (float) Math.Cos(sectorAngle);
                y = xy * (float) Math.Sin(sectorAngle);

                // Equirectangular to polar UV mapping
                float polarX = (float) Math.Atan2(x, z);
                float polarY = (float) Math.Acos(y / radius);

                u = polarX * 0.5f / (float) Math.PI + 0.5f;
                v = polarY / (float) Math.PI;

                vertices[i * (meridians + 1) + j] = new (x, y, z);
            }
        }

        int indexCount = meridians * parallels * 2 * 3;

        var indices = new IndexBuffer.Triangle[indexCount];
        int index = 0;

        for (int i = 1; i <= parallels; ++i) {
            for (int j = 1; j <= meridians; ++j) {
                int a = i * (meridians + 1) + j;
                int b = i * (meridians + 1) + j - 1;
                int c = (i - 1) * (meridians + 1) + j - 1;
                int d = (i - 1) * (meridians + 1) + j;

                indices[index++].set((ushort)c, (ushort)b, (ushort)a);
                indices[index++].set((ushort)d, (ushort)c, (ushort)a);
            }
        }

        return (vertices, indices);
    }
}