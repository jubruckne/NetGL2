using OpenTK.Mathematics;

namespace NetGL.ECS;

public static class Direction {
    public static readonly Rotation Left = new Rotation(new Vector3(-1, 0, 0));
    public static readonly Rotation Right = new Rotation(new Vector3(1, 0, 0));

    public static readonly Rotation Up = new Rotation(new Vector3(0, 1, 0));
    public static readonly Rotation Down = new Rotation(new Vector3(0, -1, 0));

    public static readonly Rotation Back = new Rotation(new Vector3(0, 0, 1));
    public static readonly Rotation Front = new Rotation(new Vector3(0, 0, -1));
}