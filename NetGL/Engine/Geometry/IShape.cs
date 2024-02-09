using OpenTK.Mathematics;

namespace NetGL;

public interface IShape <in T> {
    IEnumerable<Vector3> get_vertices();
    IEnumerable<Vector3i> get_indices();
}