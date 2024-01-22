using OpenTK.Mathematics;

namespace NetGL;

public interface IVertexSpec {
    static abstract IList<VertexAttribute> get_vertex_spec();
    void set(float x, float y, float z) => set(new Vector3(x, y, z));
    void set(in Vector3 position);
}