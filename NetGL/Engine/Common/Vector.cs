using OpenTK.Mathematics;

namespace NetGL;

public static class Vector {
    public static Vector3 right => (1, 0, 0);
    public static Vector3 left => (-1, 0, 0);
    public static Vector3 up => (0, 1, 0);
    public static Vector3 down = (0, -1, 0);
    public static Vector3 forward = (0, 0, 1);
    public static Vector3 backward = (0, 0, -1);
}