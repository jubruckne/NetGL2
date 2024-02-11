using OpenTK.Mathematics;

namespace NetGL;

public class Sphere: IShape<Sphere> {
    public readonly float radius;

    public Sphere(float radius) {
        this.radius = radius;
    }

    public IEnumerable<Vector3> get_vertices() => get_vertices(6, 6);

    public IEnumerable<Vector3> get_vertices(int meridians, int parallels) {
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

                /* // Equirectangular to polar UV mapping
                float polarX = (float) Math.Atan2(x, z);
                float polarY = (float) Math.Acos(y / radius);

                u = polarX * 0.5f / (float) Math.PI + 0.5f;
                v = polarY / (float) Math.PI;
                */
                yield return new Vector3(x, y, z);
            }
        }

        // Duplicate vertices along one seam
        for (int i = 1; i <= parallels - 1; i++) {
            float stackAngle = (float)Math.PI / 2 - i * stackStep;
            xy = radius * (float)Math.Cos(stackAngle);
            z = radius * (float)Math.Sin(stackAngle);

            yield return new Vector3(xy, 0, z); // Add an extra vertex at the seam
        }

    }

    public IEnumerable<Vector3i> get_indices() => get_indices(1f, 6, 6);

    public IEnumerable<Vector3i> get_indices(float radius, int meridians, int parallels) {
        // Calculate the total vertices per row, considering the extra vertex at the seam for each row
        int verticesPerRow = meridians + 1;

        for (int i = 1; i <= parallels; ++i) {
            for (int j = 1; j <= meridians; ++j) {
                int a = i * (meridians + 1) + j;
                int b = i * (meridians + 1) + j - 1;
                int c = (i - 1) * (meridians + 1) + j - 1;
                int d = (i - 1) * (meridians + 1) + j;

                // Adjust for extra seam vertices
                // Check if we're at the last meridian and adjust indices to wrap correctly
                if (j == meridians) {
                    a = i * verticesPerRow; // Wrap to the first vertex of the current row
                    d = (i - 1) * verticesPerRow; // Wrap to the first vertex of the previous row
                }

                yield return (c, b, a);
                yield return (d, c, a);
            }
        }
    }

    public override string ToString() {
        return $"Sphere[radius:{radius}]";
    }
}