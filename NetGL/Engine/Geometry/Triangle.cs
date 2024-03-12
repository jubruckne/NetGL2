using OpenTK.Mathematics;

namespace NetGL;

public class Triangle: IShape {
    public float p1, p2, p3;

    public Triangle(float p1, float p2, float p3) {
        this.p1 = p1;
        this.p2 = p2;
        this.p3 = p3;
    }

    public IEnumerable<Vector3> get_vertices() {
        throw new NotImplementedException();
    }

    public IEnumerable<Vector3i> get_indices() {
        throw new NotImplementedException();
    }

    public IShapeGenerator generate() {
        throw new NotImplementedException();
    }

    public override string ToString() {
        return $"Triangle[p1:{p1}, p2:{p2}, p3:{p3}]";
    }

}