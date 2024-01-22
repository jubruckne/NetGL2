namespace NetGL;

public class Triangle: IShape<Triangle> {
    public float p1, p2, p3;

    public Triangle(float p1, float p2, float p3) {
        this.p1 = p1;
        this.p2 = p2;
        this.p3 = p3;
    }
}